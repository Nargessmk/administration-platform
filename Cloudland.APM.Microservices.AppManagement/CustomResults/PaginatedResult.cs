using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cloudland.APM.Microservices.AppManagement.CustomResults;

public sealed class PaginatedResult<T> : ActionResult
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    internal PaginatedResult(int pageNumber, int pageSize = DefaultPageSize)
    {
        Limit = pageSize;
        Page = pageNumber;

        if (Limit is < 0 or > MaxPageSize) Limit = DefaultPageSize;
        if (pageNumber < 0) Page = 0;
    }

    public int Total { get; private set; }
    public int Limit { get; private set; }
    public int Page { get; private set; }
    public List<T>? Objects { get; private set; }

    internal async Task<PaginatedResult<T>> Paginate(IQueryable<T> queryable)
    {
        Total = queryable.Count();

        if (Limit > Total)
        {
            Limit = Total;
            Page = 0;
        }

        var skip = Page * Limit;
        if (skip + Limit > Total)
        {
            skip = Total - Limit;
            Page = Total / Limit - 1;
        }

        Objects = await queryable.Skip(skip).Take(Limit).ToListAsync();
        return this;
    }
}