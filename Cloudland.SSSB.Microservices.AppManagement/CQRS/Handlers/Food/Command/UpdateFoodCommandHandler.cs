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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.Command;

public sealed class UpdateFoodCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<FoodView> foodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateFoodCommand, ResultDTO<FoodDTO>?>
{


    public async Task<ResultDTO<FoodDTO>?> Handle(UpdateFoodCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Food.Id);
        var existingFood = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingFood == null)
        {
            return ResultDTO<FoodDTO>.Failure(
                "غذا پیدا نشد",
                []
            ); 
        }
        
        if (!existingFood.AllowUpdate)
        {
            return ResultDTO<FoodDTO>.Failure(
                "غذا قابل بروزرسانی نمی باشد",
                request.Food
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.Food.title) && request.Food.title != existingFood.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.Food.title && item.Id != request.Food.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<FoodDTO>.Failure(
                    "غذا با این عنوان وجود دارد", 
                    _mapper.Map<FoodDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingFood.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingFood.LastModifiedUser = userId;
            }
        }

        existingFood.FoodCategoryId = request.Food.FoodCategoryId;
        existingFood.MealId = request.Food.MealId;
        existingFood.title = request.Food.title;
        existingFood.desc = request.Food.desc;
        
        
        var updatedFood = _repository.Update(existingFood);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        
        //update related constant
        var foodConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.Food.Id);
        var foodConstant = await constantRepository.SingleOrDefaultAsync(foodConstantQuery, cancellationToken);

        foodConstant.title = existingFood.title;
        foodConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                foodConstant.LastModifiedUser = userId;
            }
        } 
        
        constantRepository.Update(foodConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = foodViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingFood.Id);
            var viewRow = await foodViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Food)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<FoodDTO>.Success(
            _mapper.Map<FoodDTO>(updatedFood),
            $"غذا {updatedFood.title} با موفقیت بروزرسانی شد"
        );

    }
}
