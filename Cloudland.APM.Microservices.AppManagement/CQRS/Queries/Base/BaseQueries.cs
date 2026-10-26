using Cloudland.APM.DTO;
using Cloudland.APM.Models.ResultModel;
using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Queries.Base;

public record BaseQuery<T> : IRequest<IEnumerable<T>>;

public record BaseQuery<T, PK>(PK Id) : IRequest<IEnumerable<T>> where T : BaseDTO<PK>;

//T is Dto
//PK is Primmary Key

public record BaseQueryById<T, PK>(PK Id) : IRequest<T>;

public record BaseQueryPaging<T, PK>(PK Id, int PageNumber, int RowInPage)
    : IRequest<IEnumerable<T>> where T : BaseDTO<PK>;

// New base record for multiple IDs
public record BaseQueryMultipleIds<T, PK>(PK Id1, PK Id2, PK Id3, PK Id4) : IRequest<T>;

public record BaseQueryPaging<T>(int PageNumber, int PageSize) : IRequest<PagedResult<T>>;