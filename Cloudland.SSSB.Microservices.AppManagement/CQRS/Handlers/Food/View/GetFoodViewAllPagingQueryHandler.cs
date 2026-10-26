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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.View;

public sealed class GetFoodViewAllPagingQueryHandler(
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
        IRequestHandler<GetFoodViewAllPagingQuery, ResultDTO<PaginatedResult<FoodViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<FoodViewDTO>>> Handle(
        GetFoodViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = foodViewRepository.MultipleResultQuery();

            var entities = await foodViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<FoodViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<FoodViewDTO>(x)));

            return ResultDTO<PaginatedResult<FoodViewDTO>>.Success(
                paginatedResult,
                "لیست غذاها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<FoodViewDTO>>.Failure(
                $"خطا در دریافت لیست غذاها: {ex.Message}",
                []
            );
        }
    }
}
