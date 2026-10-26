using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodCalendar.View;

public sealed class GetContractFoodCalendarViewByLocationIdQueryHandler(
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
        IRequestHandler<GetContractFoodCalendarViewByLocationIdQuery, ResultDTO<List<ContractFoodCalendarViewDTO>>>
{
    public async Task<ResultDTO<List<ContractFoodCalendarViewDTO>>> Handle(
        GetContractFoodCalendarViewByLocationIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = contractFoodCalendarViewRepository.MultipleResultQuery()
                .AndFilter(item => item.LocationId == request.LocationId);

            var entities = await contractFoodCalendarViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<ContractFoodCalendarViewDTO>>(entities);

            return ResultDTO<List<ContractFoodCalendarViewDTO>>.Success(
                dtos,
                "لیست تقویم وعده‌ها بر اساس شناسه مکان با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ContractFoodCalendarViewDTO>>.Failure(
                $"خطا در دریافت لیست تقویم وعده‌ها بر اساس شناسه مکان: {ex.Message}",
                new List<ContractFoodCalendarViewDTO>()
            );
        }
    }
}
