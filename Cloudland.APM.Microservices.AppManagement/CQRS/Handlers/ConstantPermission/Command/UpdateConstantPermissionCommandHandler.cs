using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using LinqKit;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.ConstantPermission.Command;

public sealed class UpdateConstantPermissionCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<UserConstantPermission> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    EntityFrameworkCore.Repository.Interfaces.IRepository<UserConstantPermission> _constantPermissionRepository)
    : BaseHandler<UserConstantPermission, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateConstantPermissionCommand, List<UserConstantPermissionDTO>>
{
    public async Task<List<UserConstantPermissionDTO>> Handle(UpdateConstantPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("Cannot resolve user claims outside of an HTTP request context.");

        var claim = httpContext.User.Claims.Skip(10).FirstOrDefault()?.Value;
        if (claim == null)
            throw new UnauthorizedAccessException(
                "The required 'NameIdentifier' claim is missing or the user is not authenticated.");

        if (!Guid.TryParse(claim, out var userClaimId))
            throw new InvalidCastException("The user's 'NameIdentifier' claim value is not a valid GUID format.");

        var predicate = PredicateBuilder.New<UserConstantPermission>(true);
        if (request.userId.HasValue)
            predicate = predicate.And(x => x.UserId == request.userId.Value);
        if (request.groupId.HasValue)
            predicate = predicate.And(x => x.GroupId == request.groupId.Value);
        if (request.roleId.HasValue)
            predicate = predicate.And(x => x.RoleId == request.roleId.Value);

        var existsItemsQuery = _constantPermissionRepository
            .MultipleResultQuery()
            .AndFilter(predicate);

        var existsItems = await _constantPermissionRepository.SearchAsync(existsItemsQuery, cancellationToken);

        var existingIds = existsItems
            .Select(x => x.ConstantId ?? Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToArray();

        var toRemove = existsItems
            .Where(x => !request.constants.Contains(x.ConstantId ?? Guid.Empty))
            .ToList();
        if (toRemove.Count != 0)
            _constantPermissionRepository.RemoveRange(toRemove);

        var toAddIds = request.constants
            .Except(existingIds)
            .ToArray();

        var newEntities = toAddIds.Select(id => new UserConstantPermission
            {
                Id = Guid.NewGuid(),
                UserId = request.userId,
                GroupId = request.groupId,
                RoleId = request.roleId,
                CreateAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                ConstantId = id,
                UserCreated = userClaimId,
                LastModifiedUser = userClaimId,
                CustomerId = Guid.NewGuid(),
                title = String.Empty,
                Disabled = false,
                AllowDelete = false,
                hidden = false
            })
            .ToList();

        if (newEntities.Count != 0)
            _constantPermissionRepository.AddRange(newEntities);

        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var resultEntities = existsItems
            .Except(toRemove)
            .Concat(newEntities)
            .ToList();

        return _mapper.Map<List<UserConstantPermissionDTO>>(resultEntities);
    }
}