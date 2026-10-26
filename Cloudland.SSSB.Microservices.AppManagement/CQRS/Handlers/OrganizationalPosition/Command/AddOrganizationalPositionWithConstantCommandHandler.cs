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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.Command;

public sealed class AddOrganizationalPositionWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalPosition> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalPositionView> organizationalPositionViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.OrganizationalPosition, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddOrganizationalPositionWithConstantCommand, ResultDTO<OrganizationalPositionDTO>?>
{
    public async Task<ResultDTO<OrganizationalPositionDTO>?> Handle(AddOrganizationalPositionWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        // Check for duplicate title
        var queryDuplicate = _repository
            .SingleResultQuery()
            .AndFilter(item => item.title == request.OrganizationalPosition.title);
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null)
        {
            return ResultDTO<OrganizationalPositionDTO>.Failure(
                "سمت سازمانی با این عنوان از قبل موجود می باشد",
                _mapper.Map<OrganizationalPositionDTO>(duplicate)
            );
        }
        
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
        newConstant.title = request.OrganizationalPosition.title;
        newConstant.desc = request.OrganizationalPosition.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("9429130f-2c58-4d2b-8d07-e5b47807208a");
        newConstant.ConstantValue = "OrganizationalPosition";
        
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var entity = _mapper.Map<Models.OrganizationalPosition>(request.OrganizationalPosition);
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
            var viewQuery = organizationalPositionViewRepository.SingleResultQuery().AndFilter(v => v.Id == entity.Id);
            var viewRow = await organizationalPositionViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                        .Index(ElasticsearchIndexNames.OrganizationalPosition)
                        .Id(viewRow.Id)
                    , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }
        
        return ResultDTO<OrganizationalPositionDTO>.Success(_mapper.Map<OrganizationalPositionDTO>(added), "سمت سازمانی با موفقیت ایجاد شد");
    }
}


