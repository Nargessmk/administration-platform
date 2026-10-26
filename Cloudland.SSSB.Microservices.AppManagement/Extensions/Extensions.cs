using Cloudland.SSSB.Microservices.AppManagement.CustomResults;

namespace Cloudland.SSSB.Microservices.AppManagement.Extensions;

public static class Extensions
{
    public static async Task<PaginatedResult<T>> Paginate<T>(this IQueryable<T> source,
        int pageSize, int pageNumber)
    {
        return await new PaginatedResult<T>(pageNumber, pageSize).Paginate(source);
    }
}