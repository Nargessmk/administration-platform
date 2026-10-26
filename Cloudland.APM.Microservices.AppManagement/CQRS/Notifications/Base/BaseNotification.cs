using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Notifications.Base;

public record AddedNotification<T>(T Entity) : INotification;

public record UpdatedNotification<T>(T Entity) : INotification;

public record DeletedNotification<T>(T Entity) : INotification;

public record DeletedMultipleNotification<T>(IEnumerable<T> Entities) : INotification;