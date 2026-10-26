using AutoMapper;
using Cloudland.APM.DTO;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.View;

public sealed class GetLocationRecursiveAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Location, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetLocationRecursiveAllQuery, ResultDTO<List<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<List<ConstantRecursiveDTO>>> Handle(
        GetLocationRecursiveAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            // Call the recursive function
            var nodes = await _dbContext
                .GetLocationRecursive(request.RootId)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ConstantRecursiveDTO>>(nodes);

            return ResultDTO<List<ConstantRecursiveDTO>>.Success(
                dtos,
                "زیرساخت مکان‌ها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ConstantRecursiveDTO>>.Failure(
                $"خطا در دریافت زیرساخت مکان‌ها: {ex.Message}",
                new List<ConstantRecursiveDTO>()
            );
        }
    }
}
