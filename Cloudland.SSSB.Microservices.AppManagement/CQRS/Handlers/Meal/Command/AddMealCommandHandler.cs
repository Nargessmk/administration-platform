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

public sealed class AddMealCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Meal> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<MealView> mealViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Meal, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddMealCommand, ResultDTO<MealDTO>?>
{
    public async Task<ResultDTO<MealDTO>?> Handle(AddMealCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.MealCategoryId == request.Meal.MealCategoryId);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<MealDTO>.Failure(
                    " نوع وعده غذایی با این عنوان از قبل موجود می باشد",
                    _mapper.Map<MealDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newMeal = _mapper.Map<Models.Meal>(request.Meal);

            newMeal.title = "";
            newMeal.CreateAt = DateTime.Now;
            newMeal.LastModifiedAt = DateTime.Now;

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newMeal.UserCreated = userId;
                    newMeal.LastModifiedUser = userId;
                }
                else
                {
                    throw new Exception("Invalid user ID in claims");
                }
            }
            else
            {
                throw new Exception("User ID claim not found");
            }

            newMeal.CustomerId = Guid.NewGuid();
            // Set default values
            newMeal.AllowDelete = true;
            newMeal.AllowUpdate = true;
            newMeal.Disabled = false;
            newMeal.hidden = false;

            // Add organizational chart to repository
            var meal = await _repository.AddAsync(newMeal, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view using repository
            try
            {
                var viewQuery = mealViewRepository.SingleResultQuery().AndFilter(v => v.Id == newMeal.Id);
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
                _mapper.Map<MealDTO>(meal),
                "وعده غذایی با موفقیت ایجاد شد"
            );
    }
}
