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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.Command;

public sealed class AddOrganizationalChartWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalChart> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChartView> organizationalChartViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddOrganizationalChartWithConstantCommand, ResultDTO<OrganizationalChartDTO>?>
{
    public async Task<ResultDTO<OrganizationalChartDTO>?> Handle(AddOrganizationalChartWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        // Check for duplicate title
        var queryDuplicate = _repository
            .SingleResultQuery()
            .AndFilter(item => item.title == request.organizationalChart.title);
        var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
        if (duplicate != null)
        {
            return ResultDTO<OrganizationalChartDTO>.Failure(
                "سازمان با این عنوان از قبل موجود می باشد",
                _mapper.Map<OrganizationalChartDTO>(duplicate)
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
        newConstant.title = request.organizationalChart.title;
        newConstant.desc = request.organizationalChart.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("8745f9b9-900b-4c52-8bf1-7d8295ba615b");
        newConstant.ConstantValue = "OrganizationalChart";
        
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var entity = _mapper.Map<Models.OrganizationalChart>(request.organizationalChart);
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
            var viewQuery = organizationalChartViewRepository.SingleResultQuery().AndFilter(v => v.Id == entity.Id);
            var viewRow = await organizationalChartViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                        .Index(ElasticsearchIndexNames.OrganizationalChart)
                        .Id(viewRow.Id)
                    , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        
        return ResultDTO<OrganizationalChartDTO>.Success(_mapper.Map<OrganizationalChartDTO>(added), "چارت سازمانی با موفقیت ایجاد شد");
    }
}


