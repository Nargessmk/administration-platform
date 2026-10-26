using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.Models.View;
using Cloudland.APM.BackgroundTask.Extensions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Cloudland.APM.BackgroundTask.Services;

public interface IReindexJobService
{
    Task ExecuteReindexAllJobAsync();
    Task ExecuteReindexByViewJobAsync(string viewName);
}

public class ReindexJobService : IReindexJobService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly SSSBDBContext _dbContext;
    private readonly ILogger<ReindexJobService> _logger;

    // Mapping between view names and their corresponding view types and index names
    private readonly Dictionary<string, (Type ViewType, string IndexName)> _viewMappings = new()
    {
        { "Personnel", (typeof(PersonnelView), ElasticsearchIndexNames.Personnel) },
        { "Vehicle", (typeof(VehicleView), ElasticsearchIndexNames.Vehicle) },
        { "Location", (typeof(LocationView), ElasticsearchIndexNames.Location) },
        { "Food", (typeof(FoodView), ElasticsearchIndexNames.Food) },
        { "Meal", (typeof(MealView), ElasticsearchIndexNames.Meal) },
        { "Contractor", (typeof(ContractorView), ElasticsearchIndexNames.Contractor) },
        { "WeekDay", (typeof(WeekDayView), ElasticsearchIndexNames.WeekDay) },
        { "OrganizationalChart", (typeof(OrganizationalChartView), ElasticsearchIndexNames.OrganizationalChart) },
        { "OrganizationalPosition", (typeof(OrganizationalPositionView), ElasticsearchIndexNames.OrganizationalPosition) }
    };

    public ReindexJobService(ElasticsearchClient elasticClient, SSSBDBContext dbContext, ILogger<ReindexJobService> logger)
    {
        _elasticClient = elasticClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteReindexAllJobAsync()
    {
        _logger.LogInformation("Starting scheduled reindex job for all views at {Time}", DateTime.UtcNow);
        
        try
        {
            var totalProcessed = 0;
            var totalErrors = 0;
            var results = new List<string>();

            foreach (var viewName in _viewMappings.Keys)
            {
                _logger.LogInformation("Processing view: {ViewName}", viewName);
                var result = await ReindexByViewNameAsync(viewName);
                totalProcessed += result.ProcessedCount;
                totalErrors += result.ErrorCount;
                results.Add($"{viewName}: {result.ProcessedCount} processed, {result.ErrorCount} errors");
                
                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to reindex view {ViewName}: {Message}", viewName, result.Message);
                }
            }

            _logger.LogInformation("Scheduled reindex job completed. Total processed: {TotalProcessed}, Total errors: {TotalErrors}", 
                totalProcessed, totalErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled reindex job execution");
            throw; // Re-throw to trigger Hangfire retry mechanism
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteReindexByViewJobAsync(string viewName)
    {
        _logger.LogInformation("Starting scheduled reindex job for view: {ViewName} at {Time}", viewName, DateTime.UtcNow);
        
        try
        {
            var result = await ReindexByViewNameAsync(viewName);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Scheduled reindex job completed for {ViewName}. Processed: {Processed}, Errors: {Errors}", 
                    viewName, result.ProcessedCount, result.ErrorCount);
            }
            else
            {
                _logger.LogError("Scheduled reindex job failed for {ViewName}: {Message}", viewName, result.Message);
                throw new InvalidOperationException($"Reindex failed for {viewName}: {result.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled reindex job execution for view: {ViewName}", viewName);
            throw; // Re-throw to trigger Hangfire retry mechanism
        }
    }

    private async Task<ReindexResult> ReindexByViewNameAsync(string viewName)
    {
        try
        {
            _logger.LogInformation("Starting reindex for view: {ViewName}", viewName);

            if (!_viewMappings.TryGetValue(viewName, out var mapping))
            {
                return ReindexResult.Failure($"View '{viewName}' is not supported for reindexing");
            }

            var (viewType, indexName) = mapping;
            var totalProcessed = 0;
            var totalErrors = 0;
            var batchSize = 100;
            var offset = 0;

            // Clear existing index first
            await _elasticClient.DeleteByQueryAsync(indexName, d => d
                .Query(q => q.MatchAll())
                .Conflicts(Elastic.Clients.Elasticsearch.Conflicts.Proceed));

            _logger.LogInformation("Cleared existing documents from index: {IndexName}", indexName);

            while (true)
            {
                // Get batch of records from the view
                var viewRecords = await GetViewRecordsBatch(viewType, offset, batchSize);
                
                if (!viewRecords.Any())
                    break;

                // Index the batch
                var bulkRequest = new BulkRequest(indexName);
                var operations = new BulkOperationsCollection();
                
                foreach (var record in viewRecords)
                {
                    operations.Add(new BulkIndexOperation<object>(record)
                    {
                        Id = GetRecordId(record).ToString()
                    });
                }
                
                bulkRequest.Operations = operations;
                var bulkResponse = await _elasticClient.BulkAsync(bulkRequest);

                if (!bulkResponse.Errors)
                {
                    totalProcessed += viewRecords.Count;
                    _logger.LogInformation("Processed batch: {Count} records, Total: {Total}", 
                        viewRecords.Count, totalProcessed);
                }
                else
                {
                    totalErrors += viewRecords.Count;
                    _logger.LogError("Failed to index batch: {Error}", bulkResponse.Errors);
                }

                offset += batchSize;

                // Small delay to prevent overwhelming the system
                await Task.Delay(100);
            }

            _logger.LogInformation("Reindex completed for {ViewName}. Processed: {Processed}, Errors: {Errors}", 
                viewName, totalProcessed, totalErrors);

            return ReindexResult.Success(totalProcessed, totalErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during reindex for view: {ViewName}", viewName);
            return ReindexResult.Failure($"Reindex failed: {ex.Message}");
        }
    }

    private async Task<List<object>> GetViewRecordsBatch(Type viewType, int offset, int batchSize)
    {
        // Use reflection to call the generic Set<T>() method with the correct type
        var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes)!
            .MakeGenericMethod(viewType);
        var dbSet = setMethod.Invoke(_dbContext, null);
        var queryable = (IQueryable<object>)dbSet!;

        return await queryable
            .OrderBy(x => EF.Property<Guid>(x, "Id"))
            .Skip(offset)
            .Take(batchSize)
            .ToListAsync();
    }

    private static Guid GetRecordId(object record)
    {
        var idProperty = record.GetType().GetProperty("Id");
        return idProperty?.GetValue(record) as Guid? ?? Guid.NewGuid();
    }
}

public class ReindexResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string>? Details { get; set; }

    public static ReindexResult Success(int processedCount, int errorCount, List<string>? details = null)
    {
        return new ReindexResult
        {
            IsSuccess = true,
            Message = "Reindex completed successfully",
            ProcessedCount = processedCount,
            ErrorCount = errorCount,
            Details = details
        };
    }

    public static ReindexResult Failure(string message)
    {
        return new ReindexResult
        {
            IsSuccess = false,
            Message = message,
            ProcessedCount = 0,
            ErrorCount = 0
        };
    }
}
