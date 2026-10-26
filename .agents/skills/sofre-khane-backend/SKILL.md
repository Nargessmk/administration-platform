---
name: sofre-khane-backend
description: Backend development guidelines, architecture, and coding standards for the sofre-khane .NET Core repository. Use this skill whenever generating or modifying backend code in this project.
---

# Sofre Khane Backend Developer Skill

## Backend Developer Role

As a developer using this skill, you are responsible for maintaining and extending the `sofre-khane` backend system. You must act as a Senior .NET Backend Engineer, strictly following the existing CQRS, Repository, and Microservices architecture. You must produce highly scalable, production-ready code that seamlessly integrates with the current Entity Framework, MediatR, and validation pipelines.

## Architecture Rules

The project follows a **Clean Architecture inspired CQRS Pattern** inside a Microservices environment.

- **Presentation Layer**: Thin ASP.NET Core API Controllers. Controllers only handle HTTP concerns, route definitions, initial model validation, and dispatching requests via MediatR.
- **Application Layer (CQRS)**: Business logic is strictly separated into `Commands` (for mutations) and `Queries` (for reads). Handlers execute the logic.
- **Data Access Layer**: A Generic Repository (`IRepository<T>`) combined with a Unit of Work (`IUnitOfWork`) abstracting Entity Framework Core.
- **Entities**: Anaemic domain models mapping directly to database tables via EF Core.

### Feature Development Workflow

When adding a new feature (e.g., "Department Management"):

1. **Entity**: Create `Department.cs` in `Cloudland.SSSB.Models`. Inherit from `BaseModel<Guid>`.
2. **Configuration**: Create `DepartmentConfiguration.cs` in `Cloudland.SSSB.DatabaseConfiguration` using EF Core Fluent API. Register it in `SSSBDBContext.cs`.
3. **DTOs & Validation**: Create `DepartmentDTO.cs` and `DepartmentDTOValidator.cs` (FluentValidation) in `Cloudland.SSSB.DTO`.
4. **Commands/Queries**: Create CQRS request records (e.g., `AddDepartmentCommand.cs`) in `CQRS/Commands/`.
5. **Handlers**: Create handlers (e.g., `AddDepartmentCommandHandler.cs`) in `CQRS/Handlers/Department/Command/`. Inject `IRepository<Department>` and `IUnitOfWork`.
6. **Controller**: Create `DepartmentController.cs`. Inject `IMediator`. Map HTTP endpoints to Mediator sends.

## Technology Stack

- **Framework**: .NET 8 (or 7) / C# 
- **Database / ORM**: SQL Server via Entity Framework Core.
- **Pattern Implementations**: 
  - **CQRS**: MediatR
  - **Repository**: `EntityFrameworkCore.Repository` package.
- **Authentication**: JWT / FusionAuth.
- **Message Broker**: RabbitMQ (via MassTransit).
- **Caching & Search**: Redis (StackExchange.Redis), ElasticSearch, RavenDB.
- **Validation**: FluentValidation (called manually in controllers via `.ValidateAndGetResult()`).
- **Auditing**: `Audit.EntityFramework` (DbContext inherits from `AuditDbContext`).

## Folder & File Organization Rules

- **Cloudland.SSSB.Models**: Contains pure Entity classes.
- **Cloudland.SSSB.DatabaseConfiguration**: Contains EF Core `IEntityTypeConfiguration<T>` classes for Entity mapping.
- **Cloudland.SSSB.DataBaseContext**: Contains `SSSBDBContext`. Do not clutter this file; use configurations.
- **Cloudland.SSSB.DTO**: Contains Data Transfer Objects and `Validation` folders.
- **Cloudland.SSSB.Microservices.AppManagement**: The API project.
  - `/Controller`: Contains API controllers (PascalCase, ending in `Controller`).
  - `/CQRS/Commands` & `/CQRS/Queries`: Define MediatR request records.
  - `/CQRS/Handlers/<Feature>`: Group handlers by feature, then split into `/Command` and `/Query` folders.

## Backend Coding Standards

### Naming & Style
- **Classes/Records/Interfaces**: PascalCase. Interfaces prefixed with `I`.
- **Private Fields**: camelCase prefixed with `_` (e.g., `_mediator`).
- **Dependency Injection**: Use C# Primary Constructors for Controllers and Handlers (e.g., `public class BedController(IMediator mediator) {}`).
- **Async Pattern**: Always use `async/await`. Suffix methods with `Async` when returning Tasks in interfaces, though Handlers override `Handle`. Pass `CancellationToken` down to DB calls.

## API Development Rules

- **Routing**: Use `[Route("api/v1/[controller]")]` and `[ApiVersion("1.0")]`.
- **Validation**: Validate DTOs explicitly at the top of controller actions.
- **Responses**: Always return `ResultDTO<T>`. Wrap in `Ok()` if successful, `BadRequest()` if failed.
- **Exception Handling**: Use `try-catch` blocks in every controller action. Return generic/safe errors to the client.

## Database Development Rules

- **Base Class**: Entities must inherit from `BaseModel<Guid>`.
- **Navigation Properties**: Use `[ForeignKey]` attributes in the Model for clarity, but put complex configurations (indexes, cascades) in `Cloudland.SSSB.DatabaseConfiguration`.
- **Views**: Database Views are fully supported and mapped (e.g., `BedView`, `BedViewConfiguration`, `BedViewDTO`).
- **Auditing**: Changes are automatically audited. Always set audit fields (`CreateAt`, `LastModifiedAt`, `UserCreated`, `CustomerId`) when inserting entities manually in Command Handlers.

## Service Layer (CQRS) Rules

- **Controllers are Dumb**: No business logic, no EF queries. Only parse requests, validate, and call `_mediator.Send()`.
- **Handlers do the work**: Perform duplicate checks, map DTOs to entities, enrich with user claims, insert via `_repository.AddAsync`, and save via `_unitOfWork.SaveChangesAsync`.
- **Dependency Injection**: Inject `IRepository<T>` specific to the entity being modified.

---

# REAL CODE EXAMPLES

## 1. Complete API Controller Pattern

**Why this structure?** It enforces thin controllers, manual but strict FluentValidation, and standardized `ResultDTO` responses.

```csharp
[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class BedController(
    IMediator mediator,
    IMapper mapper) : BaseController<BedDTO, Guid>(mediator, mapper, ...)
{
    private readonly BedDTOValidator _validator = new();

    [HttpPost]
    [ProducesResponseType(typeof(ResultDTO<BedDTO>), (int)HttpStatusCode.Created)]
    public virtual async Task<IActionResult> CreateBed([FromBody] BedDTO bed)
    {
        try
        {
            // 1. Fluent Validation
            var validationResult = bed.ValidateAndGetResult(_validator);
            if (!validationResult.IsSuccess) return BadRequest(validationResult);

            // 2. CQRS Command Dispatch
            var result = await _mediator.Send(new AddBedCommand(bed));
            
            // 3. Standardized Response
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
```

## 2. Model / Entity Pattern

**Why this structure?** Uses DataAnnotations for basic constraints while extending `BaseModel<Guid>` which enforces audit and state flags.

```csharp
namespace Cloudland.SSSB.Models;

public class Bed : BaseModel<Guid>
{
    [Required]
    [Description("شناسه اتاق مرتبط")]
    public Guid RoomId { get; set; }
    
    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string? BedNumber { get; set; }
}
```

## 3. Business Logic (Command Handler) Pattern

**Why this structure?** It encapsulates business rules (duplicate checking), claim extraction (getting User ID), and explicit unit-of-work transactions.

```csharp
public sealed class AddBedCommandHandler(
    IUnitOfWork unitOfWork,
    IRepository<Models.Bed> repository,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<AddBedCommand, ResultDTO<BedDTO>?>
{
    public async Task<ResultDTO<BedDTO>?> Handle(AddBedCommand request, CancellationToken cancellationToken)
    {
        // 1. Business Rule Validation
        var queryDuplicate = _repository.SingleResultQuery()
            .AndFilter(item => item.BedNumber == request.Bed.BedNumber && item.RoomId == request.Bed.RoomId);
            
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null) return ResultDTO<BedDTO>.Failure("تختی با این شماره موجود است", null);

        // 2. Mapping & State Enrichment
        var newBed = _mapper.Map<Models.Bed>(request.Bed);
        newBed.CreateAt = DateTime.Now;
        newBed.LastModifiedAt = DateTime.Now;
        newBed.CustomerId = Guid.NewGuid();
        newBed.AllowDelete = true;
        
        // Extract User from Context (Index 10 is specific to this project's Auth flow)
        var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
        newBed.UserCreated = Guid.Parse(userClaim);

        // 3. Database Operation
        var bed = await _repository.AddAsync(newBed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<BedDTO>.Success(_mapper.Map<BedDTO>(bed), "با موفقیت ایجاد شد");
    }
}
```

## 4. CQRS Query Pattern (Database Views)

**Why this structure?** We use SQL Views to denormalize data and perform `JOIN`s on the database side. This removes the need for complex `Include()` statements in EF Core and makes listing data significantly faster in Query Handlers.

### Step 4.1: SQL View Definition
Create a migration or SQL script for the view. Use `LEFT OUTER JOIN` for related tables to ensure no data is lost.
```sql
CREATE view [dbo].[viwFood] as
    SELECT f.Id,
           f.FoodCategoryId,
           fc.title AS FoodCategoryTitle,
           f.MealId,
           m.title  AS MealTitle,
           f.CreateAt,
           f.CustomerId,
           f.title
    FROM tblFood AS f
             LEFT OUTER JOIN tblConstant AS fc ON f.FoodCategoryId = fc.Id
             LEFT OUTER JOIN tblConstant AS m ON f.MealId = m.Id
GO
```

### Step 4.2: View POCO Model (`Cloudland.SSSB.Models.View`)
Create a POCO class mirroring the view columns. **Do not** inherit from `BaseModel<Guid>`.
```csharp
namespace Cloudland.SSSB.Models.View;

public class FoodView
{
    public Guid Id { get; set; }
    public Guid? FoodCategoryId { get; set; }
    public string? FoodCategoryTitle { get; set; }
    public Guid MealId { get; set; }
    public string? MealTitle { get; set; }
    public DateTime CreateAt { get; set; }
    public Guid CustomerId { get; set; }
    public string? title { get; set; }
}
```

### Step 4.3: EF Core Configuration (`Cloudland.SSSB.DatabaseConfiguration`)
Register the view in EF Core using `.ToView("ViewName")`. Use `.HasKey()` to specify the primary key or `.HasNoKey()` if none exists.
```csharp
public class FoodViewConfiguration : IEntityTypeConfiguration<FoodView>
{
    public void Configure(EntityTypeBuilder<FoodView> builder)
    {
        builder.ToView("viwFood");
        builder.HasKey(x => x.Id);
    }
}
```

### Step 4.4: View DTO (`Cloudland.SSSB.DTO`)
Create a matching DTO for returning data to the API.
```csharp
public class FoodViewDTO 
{
    public Guid Id { get; set; }
    public string? FoodCategoryTitle { get; set; }
    public string? MealTitle { get; set; }
    public string? title { get; set; }
}
```

### Step 4.5: Query Handler
Inject `IRepository<FoodView>` (the View, not the Entity). Querying is now extremely simple and fast.
```csharp
public sealed class GetFoodListQueryHandler(
    IRepository<FoodView> repository,
    IMapper mapper)
    : IRequestHandler<GetFoodListQuery, ResultDTO<List<FoodViewDTO>>?>
{
    public async Task<ResultDTO<List<FoodViewDTO>>?> Handle(GetFoodListQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.SingleResultQuery()
            .AndFilter(x => x.CustomerId == request.CustomerId);
            
        var foods = await _repository.ListAsync(query, cancellationToken);
        var mapped = _mapper.Map<List<FoodViewDTO>>(foods);
        
        return ResultDTO<List<FoodViewDTO>>.Success(mapped, "لیست با موفقیت دریافت شد");
    }
}
```
