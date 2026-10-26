using System.Text.Json;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Cloudland.APM.DTO.NodeObjects;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.Services;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Command;

public sealed class StoreJsonDataCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IHttpContextAccessor httpContextAccessor,
    IDtoTypeService dtoTypeService)
    : IRequestHandler<StoreJsonDataCommand, bool>
{
    public async Task<bool> Handle(StoreJsonDataCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new UnauthorizedAccessException("HttpContext is not available");

        var userId = GetCurrentUserId(httpContext);

        var constant = await GetConstantByIdAsync(request.NodeId, cancellationToken);
        if (constant?.ParentId == null)
            return false;

        var dtoObject = await ProcessDtoObjectAsync(request, cancellationToken);
        if (dtoObject == null)
            return false;

        SetBaseDtoFields(dtoObject, userId);

        await SaveJsonDataAsync(constant, dtoObject, cancellationToken);

        return true;
    }

    private static void ValidateRequest(StoreJsonDataCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.NodeType))
            throw new ArgumentException("Node type cannot be null or empty.", nameof(request.NodeType));

        if (string.IsNullOrWhiteSpace(request.DataObject))
            throw new ArgumentException("DataObject cannot be null or empty.", nameof(request.DataObject));
    }

    private static Guid GetCurrentUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.Claims.Skip(10).FirstOrDefault()?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims");

        return userId;
    }

    private async Task<Cloudland.APM.Models.Constant?> GetConstantByIdAsync(Guid nodeId, CancellationToken cancellationToken)
    {
        var query = repository.SingleResultQuery()
            .AndFilter(item => item.Id == nodeId);

        return await repository.SingleOrDefaultAsync(query, cancellationToken);
    }

    private async Task<object?> ProcessDtoObjectAsync(StoreJsonDataCommand request, CancellationToken cancellationToken)
    {
        var dtoTypeName = dtoTypeService.ConvertToDtoTypeName(request.NodeType!);

        if (!dtoTypeService.DtoTypeMap.TryGetValue(dtoTypeName, out var dtoType))
            throw new NotSupportedException($"The object type '{request.NodeType}' is not supported.");

        var dtoObject = JsonSerializer.Deserialize(request.DataObject, dtoType, dtoTypeService.JsonOptions);
        if (dtoObject == null)
            return null;

        await ValidateBasicObjectAsync(dtoObject, cancellationToken);

        await ValidateCustomObjectAsync(dtoObject, cancellationToken);

        return dtoObject;
    }

    private static async Task ValidateBasicObjectAsync(object dtoObject, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            var validationContext = new ValidationContext(dtoObject);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(dtoObject, validationContext, validationResults, true);

            if (isValid) return;
            var errorMessages = validationResults.Where(r => !string.IsNullOrEmpty(r.ErrorMessage))
                .Select(r => r.ErrorMessage);
            throw new ValidationException(string.Join(Environment.NewLine, errorMessages));
        }, cancellationToken);
    }

    private static async Task ValidateCustomObjectAsync(object dtoObject, CancellationToken cancellationToken)
    {
        if (dtoObject is ValidatableNodeObjectDTO<Guid> validatableDto)
        {
            await Task.Run(() =>
            {
                if (validatableDto.IsValid(validatableDto, out var customValidationErrors)) return;
                var errorMessages = customValidationErrors.Where(e => !string.IsNullOrEmpty(e));
                throw new ValidationException(string.Join(Environment.NewLine, errorMessages));
            }, cancellationToken);
        }
    }

    private static void SetBaseDtoFields(object dtoObject, Guid userId)
    {
        if (dtoObject is not BaseDTO<Guid> baseDto)
            return;

        var now = DateTime.UtcNow.ToString("O"); // ISO 8601 format

        if (baseDto.Id == Guid.Empty)
        {
            baseDto.Id = Guid.NewGuid();
            baseDto.CreateAt = now;
            baseDto.UserCreated = userId;
        }

        baseDto.LastModifiedAt = now;
        baseDto.LastModifiedUser = userId;
        baseDto.AllowDelete = false;
        baseDto.AllowUpdate = true;
        baseDto.Disabled = false;
        baseDto.hidden = false;
    }

    private async Task SaveJsonDataAsync(Cloudland.APM.Models.Constant constant, object dtoObject,
        CancellationToken cancellationToken)
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(dtoObject, dtoObject.GetType(), dtoTypeService.JsonOptions);

            constant.JsonData = serializedData;
            repository.Update(constant);

            var result = await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            if (result <= 0)
                throw new InvalidOperationException("Failed to save changes to database");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to serialize object to JSON", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save JSON data", ex);
        }
    }
}