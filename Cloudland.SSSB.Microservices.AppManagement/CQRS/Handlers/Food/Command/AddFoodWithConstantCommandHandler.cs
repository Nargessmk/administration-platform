using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Food.Command;

public sealed class AddFoodWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<FoodView> foodViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Food, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddFoodWithConstantCommand, ResultDTO<FoodDTO>?>
{
    public async Task<ResultDTO<FoodDTO>?> Handle(AddFoodWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        // Check for duplicate title
        var queryDuplicate = _repository
            .SingleResultQuery()
            .AndFilter(item => item.title == request.Food.title);
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null)
        {
            return ResultDTO<FoodDTO>.Failure(
                "غذا با این عنوان از قبل موجود می باشد",
                _mapper.Map<FoodDTO>(duplicate)
            );
        }

        var newConstant = new APM.Models.Constant
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now
        };
        var userId = Guid.Parse(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);
        newConstant.UserCreated = userId;
        newConstant.LastModifiedUser = userId;
        newConstant.CustomerId = Guid.NewGuid();
        newConstant.AllowDelete = true;
        newConstant.AllowUpdate = true;
        newConstant.Disabled = false;
        newConstant.hidden = false;
        newConstant.title = request.Food.title;
        newConstant.desc = request.Food.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("4ff1f03d-ed2b-448e-ac8b-1d0721c1913d");
        newConstant.ConstantValue = "Food";

        
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var entity = _mapper.Map<Models.Food>(request.Food);
        entity.Id = addedConstant.Id;
        entity.CreateAt = DateTime.Now;
        entity.LastModifiedAt = DateTime.Now;
        entity.UserCreated = userId;
        entity.LastModifiedUser = userId;
        entity.CustomerId = Guid.NewGuid();
        entity.AllowDelete = true;
        entity.AllowUpdate = true;
        entity.Disabled = false;
        entity.hidden = false;

        var added = await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = foodViewRepository.SingleResultQuery().AndFilter(v => v.Id == entity.Id);
            var viewRow = await foodViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Food)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<FoodDTO>.Success(_mapper.Map<FoodDTO>(added), "غذا با موفقیت ایجاد شد");
    }
}


