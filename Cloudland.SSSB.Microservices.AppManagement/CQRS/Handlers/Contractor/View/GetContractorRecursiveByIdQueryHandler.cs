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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.View;

public sealed class GetContractorRecursiveByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetContractorRecursiveByIdQuery, ResultDTO<ConstantRecursiveDTO>>
{
    public async Task<ResultDTO<ConstantRecursiveDTO>> Handle(
        GetContractorRecursiveByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var node = await _dbContext
                .GetContractorRecursive(request.RootId)
                .Where(x => x.Id == request.ChildId)
                .FirstOrDefaultAsync(cancellationToken);

            if (node is null)
            {
                return ResultDTO<ConstantRecursiveDTO>.Failure(
                    "پیمانکار مورد نظر در ساختار سلسله مراتبی یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<ConstantRecursiveDTO>(node);

            return ResultDTO<ConstantRecursiveDTO>.Success(
                dto,
                "پیمانکار سلسله مراتبی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<ConstantRecursiveDTO>.Failure(
                $"خطا در دریافت پیمانکار سلسله مراتبی: {ex.Message}",
                []
            );
        }
    }
}


