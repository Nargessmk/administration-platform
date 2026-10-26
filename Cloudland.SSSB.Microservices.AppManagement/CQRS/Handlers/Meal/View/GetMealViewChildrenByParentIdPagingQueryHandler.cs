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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Meal.View;

public sealed class GetMealViewChildrenByParentIdPagingQueryHandler(
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
        IRequestHandler<GetMealViewChildrenByParentIdPagingQuery, ResultDTO<PaginatedResult<MealViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<MealViewDTO>>> Handle(
        GetMealViewChildrenByParentIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = mealViewRepository
                .MultipleResultQuery()
                .AndFilter(x => x.ParentId == request.ParentId)
                .OrderByDescending(x => x.CreateAt);

            var entities = await mealViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<MealViewDTO>(x));

            var paginatedResult = new PaginatedResult<MealViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<MealViewDTO>>.Success(
                paginatedResult,
                "لیست فرزندان وعده غذایی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<MealViewDTO>>.Failure(
                $"خطا در دریافت لیست فرزندان وعده غذایی: {ex.Message}",
                []
            );
        }
    }
}
