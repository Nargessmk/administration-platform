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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.View;

public sealed class GetVehicleRecursiveAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Vehicle> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Vehicle, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetVehicleRecursiveAllQuery, ResultDTO<List<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<List<ConstantRecursiveDTO>>> Handle(
        GetVehicleRecursiveAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            // Call the recursive function
            var nodes = await _dbContext
                .GetVehicleRecursive(request.RootId)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ConstantRecursiveDTO>>(nodes);

            return ResultDTO<List<ConstantRecursiveDTO>>.Success(
                dtos,
                "زیرساخت خودروها با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ConstantRecursiveDTO>>.Failure(
                $"خطا در دریافت زیرساخت خودروها: {ex.Message}",
                new List<ConstantRecursiveDTO>()
            );
        }
    }
}
