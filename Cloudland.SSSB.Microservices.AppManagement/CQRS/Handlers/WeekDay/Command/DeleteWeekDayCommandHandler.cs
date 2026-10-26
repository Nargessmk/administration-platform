using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.WeekDay.Command;

public sealed class DeleteWeekDayCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteWeekDayCommandHandler> logger)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteWeekDayCommand, ResultDTO<int>?>
{
    public async Task<ResultDTO<int>?> Handle(DeleteWeekDayCommand request, CancellationToken cancellationToken)
    {
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var existing = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (existing == null)
        {
            return ResultDTO<int>.Failure("روز هفته یافت نشد", 0);
        }
        if (!existing.AllowDelete)
        {
            return ResultDTO<int>.Failure("روز هفته قابل حذف نمی‌باشد", 0);
        }

        _repository.Remove(existing);
        var count = await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
            // Remove from Elasticsearch
            try
            {
                await _elasticClient.DeleteAsync<WeekDayView>(request.Id, d => d
                        .Index(ElasticsearchIndexNames.WeekDay)
                    , cancellationToken);
            }
            catch
            {
                // ignore ES sync errors for now; logging can be added later
            }
        }
        else
        {
            logger.LogWarning($"No rows were affected when deleting WeekDay with ID {request.Id}");
        }
        
        return ResultDTO<int>.Success(count, "روز هفته با موفقیت حذف شد");
    }
}


