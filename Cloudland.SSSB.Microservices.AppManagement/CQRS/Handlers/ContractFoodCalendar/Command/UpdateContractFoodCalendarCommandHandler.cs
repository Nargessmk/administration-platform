using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodCalendar.Command;

public sealed class UpdateContractFoodCalendarCommandHandler(
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
        IRequestHandler<UpdateContractFoodCalendarCommand, ResultDTO<ContractFoodCalendarDTO>?>
{
    public async Task<ResultDTO<ContractFoodCalendarDTO>?> Handle(UpdateContractFoodCalendarCommand request, CancellationToken cancellationToken)
    {
        var existingQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.ContractFoodCalendar.Id);
        var existing = await _repository.SingleOrDefaultAsync(existingQuery, cancellationToken);
        if (existing == null)
        {
            return ResultDTO<ContractFoodCalendarDTO>.Failure(
                "تقویم غذایی پیدا نشد",
                []
            ); 
        }
        
        if (!existing.AllowUpdate)
        {
            return ResultDTO<ContractFoodCalendarDTO>.Failure(
                "تقویم غذایی قابل بروزرسانی نمی باشد",
                request.ContractFoodCalendar
            ); 
        }
        
        // Check for duplicate ContractFoodCalendar 
        if (request.ContractFoodCalendar.ContractFoodLocationId != existing.ContractFoodLocationId)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.ContractFoodLocationId == request.ContractFoodCalendar.ContractFoodLocationId && item.Id != request.ContractFoodCalendar.Id
                 && item.Date == _mapper.Map<DateTime>(request.ContractFoodCalendar.Date));
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<ContractFoodCalendarDTO>.Failure(
                    "غذا  با این عنوان از قبل تعریف شده است", 
                    _mapper.Map<ContractFoodCalendarDTO>(duplicate)
                );
            }
        }

        existing.ContractFoodLocationId = request.ContractFoodCalendar.ContractFoodLocationId;
        existing.DayTypeId = request.ContractFoodCalendar.DayTypeId;
        existing.IsDefault = request.ContractFoodCalendar.IsDefault;
        existing.IsHoliday = request.ContractFoodCalendar.IsHoliday;
        existing.Date = _mapper.Map<DateTime>(request.ContractFoodCalendar.Date);
        existing.desc = request.ContractFoodCalendar.desc;
        existing.MealId = request.ContractFoodCalendar.MealId;
        existing.Priority = request.ContractFoodCalendar.Priority;
        existing.LastModifiedAt = DateTime.Now;

        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<ContractFoodCalendarDTO>.Success(
            _mapper.Map<ContractFoodCalendarDTO>(existing),
            "تقویم وعده با موفقیت بروزرسانی شد"
        );
    }
}


