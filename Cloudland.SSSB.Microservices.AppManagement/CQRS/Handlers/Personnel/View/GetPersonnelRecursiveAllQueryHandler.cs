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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.View;

public sealed class GetPersonnelRecursiveAllQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetPersonnelRecursiveAllQuery, ResultDTO<List<ConstantRecursiveDTO>>>
{
    public async Task<ResultDTO<List<ConstantRecursiveDTO>>> Handle(
        GetPersonnelRecursiveAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                             ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            // Call the recursive function
            var nodes = await _dbContext
                .GetPersonnelRecursive(request.RootId)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<ConstantRecursiveDTO>>(nodes);

            return ResultDTO<List<ConstantRecursiveDTO>>.Success(
                dtos,
                "زیرساخت پرسنل با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<ConstantRecursiveDTO>>.Failure(
                $"خطا در دریافت زیرساخت پرسنل: {ex.Message}",
                new List<ConstantRecursiveDTO>()
            );
        }
    }
}
