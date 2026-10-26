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

public sealed class AddFoodCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<FoodView> foodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<AddFoodCommandHandler> logger)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddFoodCommand, ResultDTO<FoodDTO>?>
{
    public async Task<ResultDTO<FoodDTO>?> Handle(AddFoodCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.Food.title);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                logger.LogInformation($"Food with this title is existed Bfore!");
                return ResultDTO<FoodDTO>.Failure(
                    "غذا با این عنوان از قبل موجود می باشد",
                    _mapper.Map<FoodDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newFood = _mapper.Map<Models.Food>(request.Food);

            newFood.CreateAt = DateTime.Now;
            newFood.LastModifiedAt = DateTime.Now;
        

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newFood.UserCreated = userId;
                    newFood.LastModifiedUser = userId;
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

            newFood.CustomerId = Guid.NewGuid();
            // Set default values
            newFood.AllowDelete = true;
            newFood.AllowUpdate = true;
            newFood.Disabled = false;
            newFood.hidden = false;

            // Add organizational chart to repository
            var food = await _repository.AddAsync(newFood, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view using repository
            try
            {
                var viewQuery = foodViewRepository.SingleResultQuery().AndFilter(v => v.Id == newFood.Id);
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
                _mapper.Map<FoodDTO>(food),
                "غذا با موفقیت ایجاد شد"
            );
    }
}
