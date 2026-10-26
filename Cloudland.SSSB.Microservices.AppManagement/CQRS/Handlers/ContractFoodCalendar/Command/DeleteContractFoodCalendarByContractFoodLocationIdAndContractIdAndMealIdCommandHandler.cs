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

public sealed class DeleteContractFoodCalendarByContractFoodLocationIdAndContractIdAndMealIdCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFood> contractFoodRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodLocation> contractFoodLocationRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteContractFoodCalendarByContractFoodLocationIdAndContractIdAndMealIdCommand, ResultDTO<int>>
{
    public async Task<ResultDTO<int>> Handle(DeleteContractFoodCalendarByContractFoodLocationIdAndContractIdAndMealIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dateTime = _mapper.Map<DateTime>(request.Date);
            
            var contractFoodQuery = contractFoodRepository.MultipleResultQuery()
                .AndFilter(cf => cf.ContractId == request.ContractId)
                .AndFilter(cf => cf.FoodId == request.FoodId);

            var contractFoods = await contractFoodRepository.SearchAsync(contractFoodQuery, cancellationToken);
            if (!contractFoods.Any())
            {
                return ResultDTO<int>.Failure("غذای قرارداد با این مشخصات پیدا نشد", 0);
            }

            var contractFoodIds = contractFoods.Select(cf => cf.Id).ToList();
            
            var cflQuery = contractFoodLocationRepository.MultipleResultQuery()
                .AndFilter(cfl => cfl.ContractFoodId != null && contractFoodIds.Contains(cfl.ContractFoodId.Value))
                .AndFilter(cfl => cfl.ContractLocation != null && cfl.ContractLocation.LocationId == request.LocationId);

            var contractFoodLocations = await contractFoodLocationRepository.SearchAsync(cflQuery, cancellationToken);
            if (!contractFoodLocations.Any())
            {
                return ResultDTO<int>.Failure("مکان‌های غذای قرارداد برای حذف یافت نشد", 0);
            }

            var contractFoodLocationIds = contractFoodLocations.Select(cfl => cfl.Id).ToList();
            
            var query = _repository.MultipleResultQuery()
                .AndFilter(item => item.ContractFoodLocationId != null && contractFoodLocationIds.Contains(item.ContractFoodLocationId.Value))
                .AndFilter(item => item.MealId == request.MealId)
                .AndFilter(item => item.Date.Date == dateTime.Date);
            
            var entities = await _repository.SearchAsync(query, cancellationToken);
            

            if (!entities.Any())
            {
                return ResultDTO<int>.Failure(
                    "تقویم وعده با این مشخصات برای حذف پیدا نشد",
                    0
                );
            }

            var deletedCount = 0;
            foreach (var entity in entities)
            {
                await _repository.RemoveAsync(x => x.Id == entity.Id, cancellationToken);
                deletedCount++;
            }
            
            var rows = await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            
            return ResultDTO<int>.Success(
                rows, 
                $"تعداد {deletedCount} تقویم وعده با موفقیت حذف شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<int>.Failure(
                $"خطا در حذف تقویم وعده‌ها: {ex.Message}",
                0
            );
        }
    }
}
