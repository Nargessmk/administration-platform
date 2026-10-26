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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Meal.Command;

public sealed class AddMealWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Meal> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<MealView> mealViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Meal, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddMealWithConstantCommand, ResultDTO<MealDTO>?>
{
    public async Task<ResultDTO<MealDTO>?> Handle(AddMealWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        // Check for duplicate title
        var queryDuplicate = _repository
            .SingleResultQuery()
            .AndFilter(item => item.MealCategoryId == request.Meal.MealCategoryId);
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null)
        {
            return ResultDTO<MealDTO>.Failure(
                " نوع وعده غذایی با این عنوان از قبل موجود می باشد",
                _mapper.Map<MealDTO>(duplicate)
            );
        }
        
        var mealConstantQuery = constantRepository
            .SingleResultQuery()
            .AndFilter(item => item.Id == request.Meal.MealCategoryId);
        var mealConstant = await constantRepository.SingleOrDefaultAsync(mealConstantQuery, cancellationToken);
        
        var userId = Guid.Parse(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);
        var newConstant = new APM.Models.Constant
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now,
            UserCreated = userId,
            LastModifiedUser = userId,
            CustomerId = Guid.NewGuid(),
            AllowDelete = true,
            AllowUpdate = true,
            Disabled = false,
            hidden = false
        };
        
        newConstant.title = mealConstant.title;
        newConstant.desc = request.Meal.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("c0983bd4-b314-44d8-aaa4-db3bd3d82ff7");
        newConstant.ConstantValue = "Meal";
        
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var entity = _mapper.Map<Models.Meal>(request.Meal);
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
        entity.title = mealConstant.title;

        var added = await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = mealViewRepository.SingleResultQuery().AndFilter(v => v.Id == entity.Id);
            var viewRow = await mealViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Meal)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<MealDTO>.Success(_mapper.Map<MealDTO>(added), "وعده با موفقیت ایجاد شد");
    }
}


