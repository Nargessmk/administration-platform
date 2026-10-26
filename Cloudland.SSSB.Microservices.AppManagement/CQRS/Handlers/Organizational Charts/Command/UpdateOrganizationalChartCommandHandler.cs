using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Models;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using Microsoft.EntityFrameworkCore;
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.Command;

public sealed class UpdateOrganizationalChartCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChart> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChartView> organizationalChartViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateOrganizationalChartCommand, ResultDTO<OrganizationalChartDTO>?>
{
    public async Task<ResultDTO<OrganizationalChartDTO>?> Handle(UpdateOrganizationalChartCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.organizationalChart.Id);
        var existingOrganization = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingOrganization == null)
        {
            return ResultDTO<OrganizationalChartDTO>.Failure(
                "سازمان پیدا نشد",
                []
            ); 
        }
        
        if (!existingOrganization.AllowUpdate)
        {
            return ResultDTO<OrganizationalChartDTO>.Failure(
                "سازمان قابل بروزرسانی نمی باشد",
                request.organizationalChart
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.organizationalChart.title) && request.organizationalChart.title != existingOrganization.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.organizationalChart.title && item.Id != request.organizationalChart.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<OrganizationalChartDTO>.Failure(
                    "سازمان با این عنوان وجود دارد", 
                    _mapper.Map<OrganizationalChartDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingOrganization.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingOrganization.LastModifiedUser = userId;
            }
        }


        existingOrganization.title = request.organizationalChart.title;
        existingOrganization.desc = request.organizationalChart.desc;
        existingOrganization.Head = request.organizationalChart.Head;
        existingOrganization.Alternative = request.organizationalChart.Alternative;
        existingOrganization.PhoneNumber = request.organizationalChart.PhoneNumber;
        existingOrganization.Body = request.organizationalChart.Body;
        
        
        var updatedOrganization = _repository.Update(existingOrganization);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        
        //update related constant
        var organizationalChartConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.organizationalChart.Id);
        var organizationalChartConstant = await constantRepository.SingleOrDefaultAsync(organizationalChartConstantQuery, cancellationToken);

        organizationalChartConstant.title = existingOrganization.title;
        organizationalChartConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                organizationalChartConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(organizationalChartConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = organizationalChartViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingOrganization.Id);
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

        return ResultDTO<OrganizationalChartDTO>.Success(
            _mapper.Map<OrganizationalChartDTO>(updatedOrganization),
            $"سازمان {updatedOrganization.title} با موفقیت بروزرسانی شد"
        );

    }
}
