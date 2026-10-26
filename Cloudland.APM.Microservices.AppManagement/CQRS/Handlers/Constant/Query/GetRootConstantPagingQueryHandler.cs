using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Models.ResultModel;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;

public sealed class GetRootConstantPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetRootConstantPagingQuery, PagedResult<ConstantDTO>>
{
    public async Task<PagedResult<ConstantDTO>> Handle(GetRootConstantPagingQuery request,
        CancellationToken cancellationToken)
    {
        var totalCount = await _repository.CountAsync(x => x.ParentId == request.RootId, cancellationToken);
        
        var query = _repository.MultipleResultQuery()
            .AndFilter(x => x.ParentId == request.RootId)
            .OrderBy(x => x.Priority)
            .Page(request.PageNumber, request.PageSize);
            
        var items = await _repository.SearchAsync(query, cancellationToken);
        
        return new PagedResult<ConstantDTO>
        {
            Items = _mapper.Map<List<ConstantDTO>>(items),
            TotalCount = totalCount,
            PageIndex = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}