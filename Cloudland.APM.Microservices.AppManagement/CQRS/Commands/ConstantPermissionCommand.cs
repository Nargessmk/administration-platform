using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Commands;

public sealed record UpdateConstantPermissionCommand(Guid? userId, Guid? roleId, Guid? groupId, Guid[] constants)
    : UpdateCommand<Guid[], List<UserConstantPermissionDTO>>(constants);