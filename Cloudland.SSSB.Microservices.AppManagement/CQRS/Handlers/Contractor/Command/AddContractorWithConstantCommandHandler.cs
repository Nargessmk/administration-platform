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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.Command;

public sealed class AddContractorWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractorView> contractorViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddContractorWithConstantCommand, ResultDTO<ContractorDTO>?>
{
    public async Task<ResultDTO<ContractorDTO>?> Handle(AddContractorWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        // Check for duplicate title
        var queryDuplicate = _repository
            .SingleResultQuery()
            .AndFilter(item => item.title == request.Contractor.title);
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null)
        {
            return ResultDTO<ContractorDTO>.Failure(
                "پیمانکار با این عنوان از قبل موجود می باشد",
                _mapper.Map<ContractorDTO>(duplicate)
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
        newConstant.title = request.Contractor.title;
        newConstant.desc = request.Contractor.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("6d63855a-5473-49b2-8208-a34e36818e06");
        newConstant.ConstantValue = "Contractor";

        
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var entity = _mapper.Map<Models.Contractor>(request.Contractor);
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
        
        // Sync to Elasticsearch from view
        try
        {
            var viewQuery = contractorViewRepository.SingleResultQuery().AndFilter(v => v.Id == entity.Id);
            var viewRow = await contractorViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
                  
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                        .Index(ElasticsearchIndexNames.Contractor)
                        .Id(viewRow.Id)
                    , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }
        
        return ResultDTO<ContractorDTO>.Success(_mapper.Map<ContractorDTO>(added), "پیمانکار با موفقیت ایجاد شد");
    }
}


