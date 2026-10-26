using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodCalendar.View;

public sealed class GetContractFoodCalendarViewAllPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodCalendarView> ContractFoodCalendarViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodCalendarViewAllPagingQuery, ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>> Handle(
        GetContractFoodCalendarViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ContractFoodCalendarViewRepository.MultipleResultQuery();
            var entities = await ContractFoodCalendarViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<ContractFoodCalendarViewDTO>(x));

            var paginatedResult = new PaginatedResult<ContractFoodCalendarViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>.Success(
                paginatedResult,
                "لیست تقویم وعده‌ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>.Failure(
                $"خطا در دریافت لیست تقویم وعده‌ها: {ex.Message}",
                []
            );
        }
    }
}
