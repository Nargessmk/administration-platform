using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore;
using Cloudland.SSSB.Models;

namespace Cloudland.SSSB.DataBaseContext.Extensions;

public static class SSSBDBContextExtensions
{
    public static void ConfigureDatabaseFunctions(this ModelBuilder modelBuilder)
    {
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetOrganizationalChartRecursive), "GetOrganizationalChartRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetLocationRecursive), "GetLocationRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetPersonnelRecursive), "GetPersonnelRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetVehicleRecursive), "GetVehicleRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetOrganizationalPositionRecursive), "GetOrganizationalPositionRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetFoodRecursive), "GetFoodRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetMealRecursive), "GetMealRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetContractorRecursive), "GetContractorRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetCalendarRecursive), "GetCalendarRecursive");
        ConfigureRecursiveFunction<Constant>(modelBuilder, nameof(SSSBDBContext.GetWeekDayRecursive), "GetWeekDayRecursive");
        ConfigureRecursiveFunctionWithoutParameter<ContractRecursive>(modelBuilder, nameof(SSSBDBContext.GetContractRecursive), "GetContractRecursive");
    }

    private static void ConfigureRecursiveFunction<T>(ModelBuilder modelBuilder, string methodName, string functionName)
    {
        var method = typeof(SSSBDBContext)
            .GetMethod(methodName, [typeof(Guid)]);
        
        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found on '{nameof(SSSBDBContext)}'. Ensure its signature matches.");
        }
        
        modelBuilder.HasDbFunction(method)
            .HasName(functionName);
    }

    private static void ConfigureRecursiveFunctionWithoutParameter<T>(ModelBuilder modelBuilder, string methodName, string functionName)
    {
        var method = typeof(SSSBDBContext)
            .GetMethod(methodName, Type.EmptyTypes);
        
        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found on '{nameof(SSSBDBContext)}'. Ensure its signature matches.");
        }
        
        modelBuilder.HasDbFunction(method)
            .HasName(functionName);
    }
}
