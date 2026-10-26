using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cloudland.SSSB.Microservices.AppManagement.CustomResults;

public sealed class PaginatedResult<T> : ActionResult
{
    private const int DefaultPageSize = 20;

    internal PaginatedResult(int pageNumber, int pageSize = DefaultPageSize)
    {
        Limit = pageSize;
        Page = pageNumber;

        if (Limit <= 0) Limit = DefaultPageSize;
        if (Page < 1) Page = 1;
    }

    public int Total { get; private set; }
    public int Limit { get; private set; }
    public int Page { get; private set; }
    public List<T>? Objects { get; private set; }

    internal async Task<PaginatedResult<T>> Paginate(IQueryable<T> queryable)
    {
        Total = queryable.Count();

        var zeroBasedPage = Page - 1;
        var skip = zeroBasedPage * Limit;

        if (Total == 0 || skip >= Total)
        {
            Objects = new List<T>();
            return this;
        }

        var remaining = Total - skip;
        var takeCount = remaining < Limit ? remaining : Limit;

        Objects = await queryable.Skip(skip).Take(takeCount).ToListAsync();
        return this;
    }

    internal Task<PaginatedResult<T>> PaginateFromEnumerable(IEnumerable<T> enumerable)
    {
        var items = enumerable.ToList();
        Total = items.Count;

        var zeroBasedPage = Page - 1;
        var skip = zeroBasedPage * Limit;

        if (Total == 0 || skip >= Total)
        {
            Objects = new List<T>();
            return Task.FromResult(this);
        }

        var remaining = Total - skip;
        var takeCount = remaining < Limit ? remaining : Limit;

        Objects = items.Skip(skip).Take(takeCount).ToList();
        return Task.FromResult(this);
    }
}