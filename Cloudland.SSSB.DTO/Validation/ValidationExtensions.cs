using FluentValidation;
using FluentValidation.Results;

namespace Cloudland.SSSB.DTO.Validation;

public static class ValidationExtensions
{
    /// <summary>
    /// Validates a DTO and returns validation result
    /// </summary>
    /// <typeparam name="T">The DTO type</typeparam>
    /// <param name="dto">The DTO to validate</param>
    /// <param name="validator">The validator instance</param>
    /// <returns>Validation result with success status and error messages</returns>
    public static ValidationResult ValidateDTO<T>(this T dto, IValidator<T> validator)
    {
        return validator.Validate(dto);
    }

    /// <summary>
    /// Validates a DTO and returns a ResultDTO with validation errors
    /// </summary>
    /// <typeparam name="T">The DTO type</typeparam>
    /// <param name="dto">The DTO to validate</param>
    /// <param name="validator">The validator instance</param>
    /// <returns>ResultDTO with validation result</returns>
    public static ResultDTO<T> ValidateAndGetResult<T>(this T dto, IValidator<T> validator)
    {
        var validationResult = validator.Validate(dto);
        
        if (validationResult.IsValid)
        {
            return ResultDTO<T>.Success(dto, "اعتبارسنجی موفقیت آمیز بود");
        }

        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return ResultDTO<T>.Failure("خطا در اعتبارسنجی", errors);
    }

    /// <summary>
    /// Validates a DTO and throws ValidationException if invalid
    /// </summary>
    /// <typeparam name="T">The DTO type</typeparam>
    /// <param name="dto">The DTO to validate</param>
    /// <param name="validator">The validator instance</param>
    public static void ValidateAndThrow<T>(this T dto, IValidator<T> validator)
    {
        var validationResult = validator.Validate(dto);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
