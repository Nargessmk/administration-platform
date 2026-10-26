namespace Cloudland.APM.Microservices.AppManagement.Services;

public interface IReindexService
{
    Task<ReindexResult> ReindexByViewNameAsync(string viewName, CancellationToken cancellationToken = default);
    Task<ReindexResult> ReindexAllAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAvailableViewNamesAsync();
}
