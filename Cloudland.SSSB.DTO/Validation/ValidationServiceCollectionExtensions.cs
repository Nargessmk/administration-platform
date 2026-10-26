using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Cloudland.SSSB.DTO.Validation;

public static class ValidationServiceCollectionExtensions
{
    /// <summary>
    /// Registers all validators in the SSSB DTO project
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSSSBValidators(this IServiceCollection services)
    {
        // Register all validators from the current assembly
        services.AddValidatorsFromAssemblyContaining<OrganizationalChartDTOValidator>();
        
        return services;
    }

    /// <summary>
    /// Registers a specific validator
    /// </summary>
    /// <typeparam name="T">The DTO type</typeparam>
    /// <typeparam name="TValidator">The validator type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddValidator<T, TValidator>(this IServiceCollection services)
        where TValidator : class, IValidator<T>
    {
        services.AddScoped<IValidator<T>, TValidator>();
        return services;
    }
}
