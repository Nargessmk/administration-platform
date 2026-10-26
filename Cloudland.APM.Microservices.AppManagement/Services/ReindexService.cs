using Cloudland.APM.DataBaseContext;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Cloudland.APM.Microservices.AppManagement.Services;

public class ReindexService : IReindexService
{
    private readonly IElasticClient _elasticClient;
    private readonly APMDBContext _dbContext;
    private readonly ILogger<ReindexService> _logger;

    // Mapping between view names and their corresponding view types and index names
    private readonly Dictionary<string, (Type ViewType, string IndexName)> _viewMappings = new()
    {
        { "Constant", (typeof(Constant), ElasticsearchIndexNames.Constant) },
        { "ConstantRecursive", (typeof(ConstantRecursive), ElasticsearchIndexNames.Constant) }
    };

    public ReindexService(IElasticClient elasticClient, APMDBContext dbContext, ILogger<ReindexService> logger)
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

                // Convert to DTOs for indexing
                var dtoRecords = ConvertToDtos(viewRecords, viewType);

                // Index the batch
                var bulkResponse = await _elasticClient.BulkAsync(b => b
                    .Index(indexName)
                    .IndexMany(dtoRecords, (descriptor, record) => descriptor
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
        if (viewType == typeof(Constant))
        {
            var records = await _dbContext.Constants
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
            return records.Cast<object>().ToList();
        }
        else if (viewType == typeof(ConstantRecursive))
        {
            var records = await _dbContext.ConstantRecursives
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
            return records.Cast<object>().ToList();
        }

        throw new NotSupportedException($"View type {viewType.Name} is not supported");
    }

    private List<object> ConvertToDtos(List<object> records, Type viewType)
    {
        if (viewType == typeof(Constant))
        {
            return records.Cast<Constant>()
                .Select(c => new ConstantDTO
                {
                    Id = c.Id,
                    title = c.title,
                    ParentId = c.ParentId,
                    ParentConstant = null, // Required member - set to null for reindex
                    ValidStartDate = c.ValidStartDate?.ToString("O"),
                    ValidEndDate = c.ValidEndDate?.ToString("O"),
                    ValueType = c.ValueType,
                    ConstantType = c.ConstantType,
                    ConstantValue = c.ConstantValue,
                    Icon = c.Icon,
                    Color = c.Color,
                    JsonData = c.JsonData,
                    ChildMustBeShow = c.ChildMustBeShow,
                    NodeTypeId = c.NodeTypeId,
                    CreateAt = c.CreateAt.ToString("O"),
                    LastModifiedAt = c.LastModifiedAt.ToString("O"),
                    UserCreated = c.UserCreated,
                    LastModifiedUser = c.LastModifiedUser,
                    CustomerId = c.CustomerId.ToString(),
                    AllowDelete = c.AllowDelete,
                    AllowUpdate = c.AllowUpdate,
                    Disabled = c.Disabled,
                    hidden = c.hidden,
                    desc = c.desc
                })
                .Cast<object>()
                .ToList();
        }
        else if (viewType == typeof(ConstantRecursive))
        {
            return records.Cast<ConstantRecursive>()
                .Select(cr => new ConstantDTO
                {
                    Id = cr.Id,
                    title = cr.title,
                    ParentId = cr.ParentId,
                    ParentConstant = null, // Required member - set to null for reindex
                    ValidStartDate = cr.ValidStartDate?.ToString("O"),
                    ValidEndDate = cr.ValidEndDate?.ToString("O"),
                    ValueType = cr.ValueType,
                    ConstantType = null, // Required member - set to null for ConstantRecursive
                    ConstantValue = cr.ConstantValue,
                    Icon = cr.Icon,
                    Color = cr.Color,
                    NodeTypeId = cr.NodeTypeId,
                    CreateAt = cr.CreateAt.ToString("O"),
                    LastModifiedAt = cr.LastModifiedAt.ToString("O"),
                    UserCreated = cr.UserCreated,
                    LastModifiedUser = cr.LastModifiedUser,
                    CustomerId = cr.CustomerId.ToString(),
                    AllowDelete = cr.AllowDelete,
                    AllowUpdate = cr.AllowUpdate,
                    Disabled = cr.Disabled,
                    hidden = cr.hidden,
                    desc = cr.desc
                })
                .Cast<object>()
                .ToList();
        }

        throw new NotSupportedException($"View type {viewType.Name} is not supported for DTO conversion");
    }

    private static Guid GetRecordId(object record)
    {
        var idProperty = record.GetType().GetProperty("Id");
        return idProperty?.GetValue(record) as Guid? ?? Guid.NewGuid();
    }
}
