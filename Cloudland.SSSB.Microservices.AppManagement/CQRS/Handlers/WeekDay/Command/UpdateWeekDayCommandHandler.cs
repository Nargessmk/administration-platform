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

public sealed class UpdateWeekDayCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<WeekDayView> weekDayViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateWeekDayCommand, ResultDTO<WeekDayDTO>?>
{
    public async Task<ResultDTO<WeekDayDTO>?> Handle(UpdateWeekDayCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.WeekDay.Id);
        var existing = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (existing == null)
        {
            return ResultDTO<WeekDayDTO>.Failure("روز هفته یافت نشد", []);
        }
        if (!existing.AllowUpdate)
        {
            return ResultDTO<WeekDayDTO>.Failure("روز هفته قابل بروزرسانی نمی‌باشد", request.WeekDay);
        }

        existing.LastModifiedAt = DateTime.Now;
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existing.LastModifiedUser = userId;
            }
        }

        existing.title = "";
        existing.desc = request.WeekDay.desc;
        existing.StartTime = request.WeekDay.StartTime;
        existing.EndTime = request.WeekDay.EndTime;

        var updated = _repository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = weekDayViewRepository.SingleResultQuery().AndFilter(v => v.Id == existing.Id);
            var viewRow = await weekDayViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                        .Index(ElasticsearchIndexNames.WeekDay)
                        .Id(viewRow.Id)
                    , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<WeekDayDTO>.Success(
            _mapper.Map<WeekDayDTO>(updated),
            $"روز هفته {updated.title} با موفقیت بروزرسانی شد"
        );
    }
}


