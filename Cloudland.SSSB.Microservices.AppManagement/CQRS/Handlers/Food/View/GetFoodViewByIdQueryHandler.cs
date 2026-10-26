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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.View;

public sealed class GetFoodViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<FoodView> foodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetFoodViewByIdQuery, ResultDTO<FoodViewDTO>>
{
    public async Task<ResultDTO<FoodViewDTO>> Handle(
        GetFoodViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = foodViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);

            var entity = await foodViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<FoodViewDTO>.Failure(
                    "غذا مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<FoodViewDTO>(entity);

            return ResultDTO<FoodViewDTO>.Success(
                dto,
                "غذا با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<FoodViewDTO>.Failure(
                $"خطا در دریافت غذا: {ex.Message}",
                []
            );
        }
    }
}
