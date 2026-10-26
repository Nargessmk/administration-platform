# FluentValidation Implementation for SSSB DTOs

This folder contains FluentValidation implementations for SSSB DTOs.

## Structure

- `BaseDTOValidator.cs` - Base validator for common BaseDTO properties
- `CompanyDTOValidator.cs` - Specific validator for CompanyDTO
- `SubsidiaryCompanyDTOValidator.cs` - Specific validator for SubsidiaryCompanyDTO
- `ValidationExtensions.cs` - Extension methods for easy validation usage
- `ValidationServiceCollectionExtensions.cs` - DI registration extensions

## Features

### 1. Base Validation Rules
The `BaseDTOValidator<T, TKey>` provides common validation rules for all BaseDTO properties:

- **Title**: Required, 2-200 characters
- **Description**: Optional, max 1000 characters
- **CustomerId**: Required for new entities
- **Dates**: Cannot be in the future
- **User IDs**: Must be valid GUIDs
- **Business Rules**: LastModifiedAt must be after CreateAt

### 2. Company-Specific Validation
The `CompanyDTOValidator` extends the base validator with company-specific rules:

- **Title**: Company-specific error messages
- **ConstantId**: Optional but must be valid GUID if provided
- **ID**: Must be valid GUID for updates

### 3. SubsidiaryCompany-Specific Validation
The `SubsidiaryCompanyDTOValidator` extends the base validator with subsidiary company-specific rules:

- **Title**: Subsidiary company-specific error messages
- **CompanyId**: Required and must be a valid GUID (parent company reference)
- **ID**: Must be valid GUID for updates

### 3. Validation Extensions
Extension methods for easy validation usage:

```csharp
// Basic validation
var result = company.ValidateDTO(validator);

// Get ResultDTO with validation errors
var result = company.ValidateAndGetResult(validator);

// Throw exception if invalid
company.ValidateAndThrow(validator);
```

## Usage

### 1. Register Validators in DI Container

```csharp
// In Program.cs or Startup.cs
services.AddSSSBValidators();

// Or register specific validator
services.AddValidator<CompanyDTO, CompanyDTOValidator>();
```

### 2. Use in Controllers

```csharp
public class CompanyController : ControllerBase
{
    private readonly CompanyDTOValidator _validator = new();

    [HttpPost]
    public async Task<IActionResult> AddCompany([FromBody] CompanyDTO company)
    {
        // Validate the DTO
        var validationResult = company.ValidateAndGetResult(_validator);
        if (!validationResult.IsSuccess)
        {
            return BadRequest(validationResult);
        }

        // Continue with business logic...
    }
}
```

### 3. Use in Command Handlers

```csharp
public class AddCompanyCommandHandler : IRequestHandler<AddCompanyCommand, ResultDTO<CompanyDTO>>
{
    private readonly CompanyDTOValidator _validator = new();

    public async Task<ResultDTO<CompanyDTO>> Handle(AddCompanyCommand request, CancellationToken cancellationToken)
    {
        // Validate the DTO
        var validationResult = request.company.ValidateAndGetResult(_validator);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        // Continue with business logic...
    }
}
```

## Validation Rules

### CompanyDTO Validation Rules

| Property | Rule | Message |
|----------|------|---------|
| title | Required, 2-200 chars | "عنوان شرکت الزامی است" |
| desc | Optional, max 1000 chars | "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد" |
| CustomerId | Required for new | "شناسه مشتری الزامی است" |
| ConstantId | Valid GUID if provided | "شناسه ثابت باید یک GUID معتبر باشد" |
| Id | Valid GUID for updates | "شناسه شرکت باید یک GUID معتبر باشد" |

### SubsidiaryCompanyDTO Validation Rules

| Property | Rule | Message |
|----------|------|---------|
| title | Required, 2-200 chars | "عنوان شرکت فرعی الزامی است" |
| desc | Optional, max 1000 chars | "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد" |
| CustomerId | Required for new | "شناسه مشتری الزامی است" |
| CompanyId | Required, valid GUID | "شناسه شرکت اصلی الزامی است" |
| Id | Valid GUID for updates | "شناسه شرکت فرعی باید یک GUID معتبر باشد" |

### BaseDTO Validation Rules

| Property | Rule | Message |
|----------|------|---------|
| title | Required, 2-200 chars | "عنوان الزامی است" |
| desc | Optional, max 1000 chars | "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد" |
| CreateAt | Not in future | "تاریخ ایجاد نمی‌تواند در آینده باشد" |
| LastModifiedAt | Not in future | "تاریخ آخرین تغییر نمی‌تواند در آینده باشد" |
| UserCreated | Valid GUID | "شناسه کاربر ایجاد کننده الزامی است" |
| LastModifiedUser | Valid GUID | "شناسه آخرین کاربر تغییر دهنده الزامی است" |

## Error Messages

All error messages are in Persian (Farsi) to match the application's language requirements.

## Extending

To add validation for a new DTO:

1. Create a new validator class inheriting from `BaseDTOValidator<T, TKey>`
2. Add specific validation rules in the constructor
3. Register the validator in DI container
4. Use in controllers or handlers

Example:
```csharp
public class NewDTOValidator : BaseDTOValidator<NewDTO, Guid>
{
    public NewDTOValidator()
    {
        // Add specific validation rules
        RuleFor(x => x.SpecificProperty)
            .NotEmpty()
            .WithMessage("Property is required");
    }
}
```
