using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

public record AddCommand<T>(T Entity) : IRequest<ResultDTO<T>>;

public record AddCommandDifferenceOutput<T, K, PK>(T Entity) : IRequest<ResultDTO<K>>
    where T : BaseDTO<PK>;
    

public record AddCommand<T, PK>(T Entity) : IRequest<ResultDTO<PK>>;

public record UpdateCommand<T>(T Entity) : IRequest<ResultDTO<T>>;
public record UpdateCommandDifferenceOutput<T, K, PK>(T Entity) : IRequest<ResultDTO<K>>
    where T : BaseDTO<PK>;

public record UpdateCommand<T, PK>(T Entity) : IRequest<ResultDTO<PK>>;

public record DeleteCommand<PK>(PK Id) : IRequest<ResultDTO<int>>;

public record DeleteCommand<PK1, PK2>(PK1 Id, PK2 Id2) : IRequest<ResultDTO<int>>;

public record DeleteListCommand<PK>(PK[] Ids) : IRequest<ResultDTO<int>>;

public record StoreJsonDataCommand<PK>(PK NodeId, string? NodeType, string DataObject)
    : IRequest<ResultDTO<bool>>;

public record ReindexCommand<T> : IRequest<ResultDTO<long>>;