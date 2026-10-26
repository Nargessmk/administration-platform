using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DataBaseContext;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Command;

public sealed class AddConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddConstantCommand, ConstantDTO?>
{
    
    public async Task<ConstantDTO?> Handle(AddConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception();
        }
        var apmDbContext = _unitOfWork.DbContext as APMDBContext;
        if (apmDbContext == null)
        {
            throw new Exception();
        }

        var newConstant = _mapper.Map<Cloudland.APM.Models.Constant>(request.constant);
        var queryDuplicate = _repository
            .SingleResultQuery()
            .AndFilter(item => item.title == request.constant.title && item.ParentId == newConstant.ParentId);
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null) return null;

        var constantNodeQuery = _repository.SingleResultQuery()
            .AndFilter(item => item.Id == request.constant.NodeTypeId);
        var constantNode = await _repository.SingleOrDefaultAsync(constantNodeQuery, cancellationToken);
        if (constantNode == null)
            return null;

        newConstant.Id = Guid.NewGuid();
        newConstant.CreateAt = DateTime.Now;
        newConstant.LastModifiedAt = DateTime.Now;
        newConstant.UserCreated = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);
        newConstant.LastModifiedUser = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);
        newConstant.CustomerId = Guid.NewGuid();
        newConstant.AllowDelete = constantNode.AllowDelete;
        newConstant.AllowUpdate = constantNode.AllowUpdate;
        newConstant.Disabled = constantNode.Disabled;
        newConstant.hidden = constantNode.hidden;
        newConstant.ChildMustBeShow = true;

        var constant = await _repository.AddAsync(newConstant, cancellationToken); 
        await CreateFolderConstant(constant.NodeTypeId, constant.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch
        try
        {
            var constantDto = _mapper.Map<ConstantDTO>(constant);
            await _elasticClient.IndexAsync(constantDto, i => i
                .Index(ElasticsearchIndexNames.Constant)
                .Id(constantDto.Id)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return _mapper.Map<ConstantDTO>(constant);
    }

    private async Task<bool> CreateFolderConstant(Guid? constantNodeTypeId,Guid? constantId,CancellationToken cancellationToken )
    {
        
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception();
        }
        
        List<FolderItemDTO> matchingFolders = FolderConstants.FolderList
            .Where(f => f.ParentNodeTypeId == constantNodeTypeId)
            .ToList();
        if(matchingFolders.Count == 0) return false;

        foreach (var folderConstant in matchingFolders.Select(folder => new Cloudland.APM.Models.Constant()
                 {
                     Id = Guid.NewGuid(),
                     CreateAt = DateTime.Now,
                     LastModifiedAt = DateTime.Now,
                     UserCreated = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value),
                     LastModifiedUser = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value),
                     title = folder.FolderTitle,
                     Icon = folder.FolderIcon,
                     Color = "#93bee6",
                     ParentId = constantId,
                     NodeTypeId = Guid.Parse("ec640c73-0bda-42ee-b7f7-f946ea3354dd"),
                     ConstantValue = "Folder",
                     CustomerId = Guid.NewGuid(),
                     AllowDelete = false,
                     AllowUpdate = true,
                     Disabled = false,
                     hidden = false,
                     ChildMustBeShow = true
                 }))
        {
            await _repository.AddAsync(folderConstant, cancellationToken);
        }
        return true;
    }
}


