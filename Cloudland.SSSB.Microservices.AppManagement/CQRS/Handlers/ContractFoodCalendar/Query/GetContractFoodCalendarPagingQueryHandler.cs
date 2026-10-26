using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodCalendar.Query;

public sealed class GetContractFoodCalendarPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodCalendarPagingQuery, ResultDTO<PaginatedResult<ContractFoodCalendarDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractFoodCalendarDTO>>> Handle(GetContractFoodCalendarPagingQuery request, CancellationToken cancellationToken)
    {
        var pagingQuery = _repository.MultipleResultQuery()
            .Page(request.PageNumber, request.PageSize)
            .OrderByDescending(x => x.CreateAt);

        var pagedResult = await _repository.SearchAsync(pagingQuery, cancellationToken);
        var mapped = _mapper.Map<PaginatedResult<ContractFoodCalendarDTO>>(pagedResult);
        return ResultDTO<PaginatedResult<ContractFoodCalendarDTO>>.Success(mapped);
    }
}


