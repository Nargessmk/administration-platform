using Cloudland.APM.Microservices.AppManagement.CustomResults;

namespace Cloudland.APM.Microservices.AppManagement.Extensions;

public static class Extensions
{
    public static async Task<PaginatedResult<T>> Paginate<T>(this IQueryable<T> source,
        int pageSize, int pageNumber)
    {
        return await new PaginatedResult<T>(pageNumber, pageSize).Paginate(source);
    }
}