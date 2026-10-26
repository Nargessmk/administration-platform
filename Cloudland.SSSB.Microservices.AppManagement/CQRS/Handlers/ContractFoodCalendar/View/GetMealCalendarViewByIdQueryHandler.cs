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

public sealed class GetContractFoodCalendarViewByIdQueryHandler(
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
        IRequestHandler<GetContractFoodCalendarViewByIdQuery, ResultDTO<ContractFoodCalendarViewDTO>>
{
    public async Task<ResultDTO<ContractFoodCalendarViewDTO>> Handle(
        GetContractFoodCalendarViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = ContractFoodCalendarViewRepository.SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);
            
            var entity = await ContractFoodCalendarViewRepository.SingleOrDefaultAsync(query, cancellationToken);
            
            if (entity == null)
            {
                return ResultDTO<ContractFoodCalendarViewDTO>.Failure(
                    "تقویم وعده یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ContractFoodCalendarViewDTO>(entity);

            return ResultDTO<ContractFoodCalendarViewDTO>.Success(
                dto,
                "تقویم وعده با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ContractFoodCalendarViewDTO>.Failure(
                $"خطا در دریافت تقویم وعده: {ex.Message}",
                []
            );
        }
    }
}
