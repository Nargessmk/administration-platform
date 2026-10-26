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

public sealed class AddWeekDayWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<WeekDayView> weekDayViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.WeekDay, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddWeekDayWithConstantCommand, ResultDTO<WeekDayDTO>?>
{
    public async Task<ResultDTO<WeekDayDTO>?> Handle(AddWeekDayWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var newConstant = new APM.Models.Constant
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now,
            CustomerId = Guid.NewGuid(),
            AllowDelete = true,
            AllowUpdate = true,
            Disabled = false,
            hidden = false,
            title = "",
            desc = request.WeekDay.desc
        };

        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid constantUserId))
            {
                newConstant.UserCreated = constantUserId;
                newConstant.LastModifiedUser = constantUserId;
            }
        }
        
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("c4019b1e-6cba-4ba0-9eee-ae4526c26ba8");
        newConstant.ConstantValue = "WeekDay";

        await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var newEntity = _mapper.Map<Models.WeekDay>(request.WeekDay);
        newEntity.Id = newConstant.Id;
        newEntity.CreateAt = DateTime.Now;
        newEntity.LastModifiedAt = DateTime.Now;
        newEntity.title = "";

        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                newEntity.UserCreated = userId;
                newEntity.LastModifiedUser = userId;
            }
        }

        newEntity.CustomerId = newConstant.CustomerId;
        newEntity.AllowDelete = true;
        newEntity.AllowUpdate = true;
        newEntity.Disabled = false;
        newEntity.hidden = false;

        var entity = await _repository.AddAsync(newEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
         
        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = weekDayViewRepository.SingleResultQuery().AndFilter(v => v.Id == newEntity.Id);
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
            _mapper.Map<WeekDayDTO>(entity),
            "روز هفته با موفقیت ایجاد شد"
        );
    }
}


