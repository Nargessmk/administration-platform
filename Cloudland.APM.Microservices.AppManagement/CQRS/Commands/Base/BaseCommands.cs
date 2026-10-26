using Cloudland.APM.DTO;
using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Commands.Base;

public record AddCommand<T>(T Entity) : IRequest<T>;

public record AddCommandDifferenceOutput<T, K, PK>(T Entity) : IRequest<K>
    where T : BaseDTO<PK>
    where K : BaseDTO<PK>;

public record AddCommand<T, PK>(T Entity) : IRequest<PK>;

public record UpdateCommand<T>(T Entity) : IRequest<T>;

public record UpdateCommand<T, PK>(T Entity) : IRequest<PK>;

public record DeleteCommand<PK>(PK Id) : IRequest<int>;

public record DeleteCommand<PK1, PK2>(PK1 Id, PK2 Id2) : IRequest<int>;

public record DeleteListCommand<PK>(PK[] Ids) : IRequest<int>;

public record StoreJsonDataCommand<PK>(PK NodeId, string? NodeType, string DataObject)
    : IRequest<bool>;

public record ReindexCommand<T> : IRequest<long>;