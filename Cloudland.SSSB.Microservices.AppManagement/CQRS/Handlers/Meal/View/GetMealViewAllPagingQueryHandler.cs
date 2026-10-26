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

public sealed class GetMealViewAllPagingQueryHandler(
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
        IRequestHandler<GetMealViewAllPagingQuery, ResultDTO<PaginatedResult<MealViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<MealViewDTO>>> Handle(
        GetMealViewAllPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = mealViewRepository.MultipleResultQuery();

            var entities = await mealViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<MealViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<MealViewDTO>(x)));

            return ResultDTO<PaginatedResult<MealViewDTO>>.Success(
                paginatedResult,
                "لیست وعده‌های غذایی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<MealViewDTO>>.Failure(
                $"خطا در دریافت لیست وعده‌های غذایی: {ex.Message}",
                []
            );
        }
    }
}
