using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Cloudland.SSSB.Microservices.AppManagement.Services;

public interface IReindexService
{
    Task<ReindexResult> ReindexByViewNameAsync(string viewName, CancellationToken cancellationToken = default);
    Task<ReindexResult> ReindexAllAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableViewNamesAsync();
}

public class ReindexService : IReindexService
{
    private readonly IElasticClient _elasticClient;
    private readonly SSSBDBContext _dbContext;
    private readonly ILogger<ReindexService> _logger;

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

    public ReindexService(IElasticClient elasticClient, SSSBDBContext dbContext, ILogger<ReindexService> logger)
    {
        _elasticClient = elasticClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ReindexResult> ReindexByViewNameAsync(string viewName, CancellationToken cancellationToken = default)
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
            await _elasticClient.DeleteByQueryAsync<object>(d => d
                .Index(indexName)
                .Query(q => q.MatchAll())
                .Conflicts(Elasticsearch.Net.Conflicts.Proceed), cancellationToken);

            _logger.LogInformation("Cleared existing documents from index: {IndexName}", indexName);

            while (true)
            {
                // Get batch of records from the view
                var viewRecords = await GetViewRecordsBatch(viewType, offset, batchSize, cancellationToken);
                
                if (!viewRecords.Any())
                    break;

                // Index the batch
                var bulkResponse = await _elasticClient.BulkAsync(b => b
                    .Index(indexName)
                    .IndexMany(viewRecords, (descriptor, record) => descriptor
                        .Id(GetRecordId(record))
                        .Document(record)
                    ), cancellationToken);

                if (bulkResponse.IsValid)
                {
                    totalProcessed += viewRecords.Count;
                    _logger.LogInformation("Processed batch: {Count} records, Total: {Total}", 
                        viewRecords.Count, totalProcessed);
                }
                else
                {
                    totalErrors += viewRecords.Count;
                    _logger.LogError("Failed to index batch: {Error}", bulkResponse.ServerError?.Error);
                }

                offset += batchSize;

                // Small delay to prevent overwhelming the system
                await Task.Delay(100, cancellationToken);
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

    public async Task<ReindexResult> ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        var totalProcessed = 0;
        var totalErrors = 0;
        var results = new List<string>();

        foreach (var viewName in _viewMappings.Keys)
        {
            var result = await ReindexByViewNameAsync(viewName, cancellationToken);
            totalProcessed += result.ProcessedCount;
            totalErrors += result.ErrorCount;
            results.Add($"{viewName}: {result.ProcessedCount} processed, {result.ErrorCount} errors");
        }

        return ReindexResult.Success(totalProcessed, totalErrors, results);
    }

    public Task<List<string>> GetAvailableViewNamesAsync()
    {
        return Task.FromResult(_viewMappings.Keys.ToList());
    }

    private async Task<List<object>> GetViewRecordsBatch(Type viewType, int offset, int batchSize, CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);
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
