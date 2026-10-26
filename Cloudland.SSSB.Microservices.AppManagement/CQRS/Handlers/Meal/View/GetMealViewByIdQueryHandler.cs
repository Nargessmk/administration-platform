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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Meal.View;

public sealed class GetMealViewByIdQueryHandler(
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
        IRequestHandler<GetMealViewByIdQuery, ResultDTO<MealViewDTO>>
{
    public async Task<ResultDTO<MealViewDTO>> Handle(
        GetMealViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = mealViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id);

            var entity = await mealViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<MealViewDTO>.Failure(
                    "وعده غذایی مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<MealViewDTO>(entity);

            return ResultDTO<MealViewDTO>.Success(
                dto,
                "وعده غذایی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<MealViewDTO>.Failure(
                $"خطا در دریافت وعده غذایی: {ex.Message}",
                []
            );
        }
    }
}
