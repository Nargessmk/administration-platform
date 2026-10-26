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

public sealed class GetContractFoodCalendarViewByLocationIdAndContractIdPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractFoodCalendarView> contractFoodCalendarViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractFoodCalendarViewByLocationIdAndContractIdPagingQuery, ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>> Handle(
        GetContractFoodCalendarViewByLocationIdAndContractIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodCalendarViewRepository
                .MultipleResultQuery()
                .AndFilter(item => item.LocationId == request.LocationId)
                .AndFilter(item => item.ContractId == request.ContractId)
                .OrderBy(item => item.Date.Day);

            var entities = await contractFoodCalendarViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<ContractFoodCalendarViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<ContractFoodCalendarViewDTO>(x)));

            return ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>.Success(
                paginatedResult,
                "لیست صفحه‌بندی شده تقویم وعده‌ها بر اساس شناسه مکان و شناسه قرارداد با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<ContractFoodCalendarViewDTO>>.Failure(
                $"خطا در دریافت لیست صفحه‌بندی شده تقویم وعده‌ها بر اساس شناسه مکان و شناسه قرارداد: {ex.Message}",
                []
            );
        }
    }
}


