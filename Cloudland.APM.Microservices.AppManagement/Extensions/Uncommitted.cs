using System.Transactions;

namespace Cloudland.APM.Microservices.AppManagement.Extensions;

public static class Uncommitted
{
    public static List<T> ToListReadUncommitted<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            });
        var toReturn = query.ToList();
        scope.Complete();
        return toReturn;
    }

    public static int CountReadUncommitted<T>(this IQueryable<T> query)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            });
        var toReturn = query.ToList().Count;
        scope.Complete();
        return toReturn;
    }
}