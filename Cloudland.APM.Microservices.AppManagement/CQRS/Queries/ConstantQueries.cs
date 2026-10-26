using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries.Base;
using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Queries;

public sealed record GetRootConstantQuery : BaseQuery<ConstantDTO>;

public sealed record GetConstantByIdQuery(Guid Id) : BaseQueryById<ConstantDTO, Guid>(Id);

public sealed record GetTreeConstantQuery(Guid RootId) : BaseQuery<ConstantDTO>;

public sealed record GetConstantByDateQuery(Guid RootId, DateTime dateTime) : BaseQuery<ConstantRecursiveDTO>;

public sealed record GetConstantRecursiveByRootId(Guid RootId)
    : BaseQueryById<List<ConstantRecursiveDTO>, Guid>(RootId);

public sealed record GetRootConstantPagingQuery(Guid RootId, int PageNumber, int PageSize)
    : BaseQueryPaging<ConstantDTO>(PageNumber, PageSize);

public sealed record GetRootChildConstantQuery(Guid RootId)
    : BaseQueryById<List<ConstantDTO>, Guid>(RootId);

public sealed record RetrieveJsonDataQuery(Guid NodeId, string? NodeType) : BaseQueryById<object?, Guid>(NodeId);
public sealed record GetConstantNodeTypeByIdQuery(Guid Id, Guid? NodeTypeId)
    : BaseQueryById<List<ConstantDTO>, Guid>(Id);

public sealed record GetConstantsByIdsQuery(List<Guid> Ids) : IRequest<List<ConstantDTO>>;