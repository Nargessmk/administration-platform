using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Queries;

public sealed record GetConstantPermissionRecursive(
    Guid RootId,
    Guid ApplicationId,
    Guid? UserId,
    Guid? RoleId,
    Guid? GroupId
) : BaseQueryMultipleIds<List<ConstantRecursiveDTO>, Guid?>(RootId, UserId, RoleId, GroupId);