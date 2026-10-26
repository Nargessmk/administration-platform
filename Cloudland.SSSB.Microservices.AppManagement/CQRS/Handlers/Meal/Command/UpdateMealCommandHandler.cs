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
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Meal.Command;

public sealed class UpdateMealCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Meal> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<MealView> mealViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Meal, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateMealCommand, ResultDTO<MealDTO>?>
{


    public async Task<ResultDTO<MealDTO>?> Handle(UpdateMealCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Meal.Id);
        var existingMeal = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingMeal == null)
        {
            return ResultDTO<MealDTO>.Failure(
                "وعده غذایی پیدا نشد",
                []
            ); 
        }
        
        if (!existingMeal.AllowUpdate)
        {
            return ResultDTO<MealDTO>.Failure(
                "وعده غذایی قابل بروزرسانی نمی باشد",
                request.Meal
            ); 
        }

        // Check for duplicate title 
        if ( request.Meal.MealCategoryId != existingMeal.MealCategoryId)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.MealCategoryId == request.Meal.MealCategoryId && item.Id != request.Meal.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<MealDTO>.Failure(
                    "نوع وعده غذایی با این عنوان وجود دارد", 
                    _mapper.Map<MealDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        var mealCategoryConstantQuery = constantRepository
            .SingleResultQuery()
            .AndFilter(item => item.Id == request.Meal.MealCategoryId);
        var mealCategoryConstant = await constantRepository.SingleOrDefaultAsync(mealCategoryConstantQuery, cancellationToken);

        
        existingMeal.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingMeal.LastModifiedUser = userId;
            }
        }
        

        existingMeal.ActivationEndDate = _mapper.Map<DateTime?>(request.Meal.ActivationEndDate);
        existingMeal.ActivationStartDate = _mapper.Map<DateTime?>(request.Meal.ActivationStartDate);
        existingMeal.DefaultReservationDayOffset = request.Meal.DefaultReservationDayOffset;
        existingMeal.DefaultReservationTime = request.Meal.DefaultReservationTime;
        existingMeal.IsPostponed = request.Meal.IsPostponed;
        existingMeal.EndTime = request.Meal.EndTime;
        existingMeal.ReservationEndDayOffset = request.Meal.ReservationEndDayOffset;
        existingMeal.StartTime = request.Meal.StartTime;
        existingMeal.ReservationEndTime = request.Meal.ReservationEndTime;
        existingMeal.IsActive = request.Meal.IsActive;
        existingMeal.title = mealCategoryConstant.title;
        existingMeal.desc = request.Meal.desc;
        
        
        
        var updatedMeal = _repository.Update(existingMeal);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        //update related constant
        var mealConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.Meal.Id);
        var mealConstant = await constantRepository.SingleOrDefaultAsync(mealConstantQuery, cancellationToken);

        mealConstant.title = existingMeal.title;
        mealConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                mealConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(mealConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = mealViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingMeal.Id);
            var viewRow = await mealViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Meal)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<MealDTO>.Success(
            _mapper.Map<MealDTO>(updatedMeal),
            $"وعده غذایی {updatedMeal.title} با موفقیت بروزرسانی شد"
        );

    }
}
