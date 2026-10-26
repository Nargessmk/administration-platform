using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Notifications.Base;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Notifications;

public sealed record ConstantAddedNotification(ConstantDTO Entity) : AddedNotification<ConstantDTO>(Entity);

public sealed record ConstantUpdatedNotification(ConstantDTO Entity) : UpdatedNotification<ConstantDTO>(Entity);

public sealed record ConstantDeletedNotification(ConstantDTO Entity) : DeletedNotification<ConstantDTO>(Entity);

public sealed record ConstantDeletedGroupNotification(IEnumerable<ConstantDTO> Entities)
    : DeletedMultipleNotification<ConstantDTO>(Entities);