using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;

public sealed record GetContractorByIdQuery(Guid Id) : BaseQueryById<ContractorDTO, Guid>(Id);
public sealed record GetContractorPagingQuery(int PageNumber, int PageSize) : BaseQueryPaging<ContractorDTO>(PageNumber, PageSize);
