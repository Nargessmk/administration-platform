using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using io.fusionauth;
using MassTransit;
using Cloudland.APM.DataBaseContext;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.ConstantPermission.Query;

public sealed class GetConstantPermissionRecursiveHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ConstantRecursive> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    IFusionAuthAsyncClient fusionClient)
    : BaseHandler<ConstantRecursive, Guid>(unitOfWork, repository, mapper, publishEndpoint,
            elasticClient, distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetConstantPermissionRecursive, List<ConstantRecursiveDTO>>
{
    public async Task<List<ConstantRecursiveDTO>> Handle(GetConstantPermissionRecursive request,
        CancellationToken cancellationToken)
    {
         if (_unitOfWork.DbContext is not APMDBContext apmDbContext)
        throw new InvalidOperationException(
            $"The provided DbContext is not of the expected type '{nameof(APMDBContext)}'.");

    if (request.UserId == null && (request.RoleId != null || request.GroupId != null))
    {
        var resultForSingle = await apmDbContext
            .GetConstantsRecursiveWithPermissions(request.RootId, null, request.RoleId?.ToString(), request.GroupId?.ToString())
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ConstantRecursiveDTO>>(resultForSingle);
    }

    var userQuery = await fusionClient.ReactivateUserAsync(request.UserId);
    if (!userQuery.WasSuccessful()) throw new InvalidOperationException("Can not retrieve user information.");
    var userInfo = userQuery.successResponse.user;

    var applicationQuery = await fusionClient.RetrieveApplicationAsync(request.ApplicationId);
    var targetRegistration = userInfo.registrations?
        .FirstOrDefault(reg => reg.applicationId == request.ApplicationId);
    if (targetRegistration == null || !applicationQuery.WasSuccessful())
        throw new InvalidOperationException("Application not found.");

    //Collect all Role and Group IDs
    var appRoles = applicationQuery.successResponse.application.roles;
    List<string> roleNames = targetRegistration.roles;
    var groupIds = userInfo.memberships.Select(x => x.id).ToArray();
    var roleIds = appRoles.Where(x => roleNames.Contains(x.name)).Select(x => x.id).ToArray();

    //Convert ID arrays into comma-separated strings
    var roleIdsString = string.Join(",", roleIds);
    var groupIdsString = string.Join(",", groupIds);

    // The previous `permissionVariations` list and `foreach` loop are now gone.
    var results = await apmDbContext
        .GetConstantsRecursiveWithPermissions(
            request.RootId,
            request.UserId,
            roleIdsString,
            groupIdsString)
        .ToListAsync(cancellationToken);

    return _mapper.Map<List<ConstantRecursiveDTO>>(results);
    }
}