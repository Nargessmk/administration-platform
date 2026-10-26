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
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.OrganizationalPosition.Command;

public sealed class UpdateOrganizationalPositionCommandHandler(
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
        IRequestHandler<UpdateOrganizationalPositionCommand, ResultDTO<OrganizationalPositionDTO>?>
{


    public async Task<ResultDTO<OrganizationalPositionDTO>?> Handle(UpdateOrganizationalPositionCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.OrganizationalPosition.Id);
        var existingOrganizationalPosition = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingOrganizationalPosition == null)
        {
            return ResultDTO<OrganizationalPositionDTO>.Failure(
                "سمت سازمانی پیدا نشد",
                []
            ); 
        }
        
        if (!existingOrganizationalPosition.AllowUpdate)
        {
            return ResultDTO<OrganizationalPositionDTO>.Failure(
                "سمت سازمانی قابل بروزرسانی نمی باشد",
                request.OrganizationalPosition
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.OrganizationalPosition.title) && request.OrganizationalPosition.title != existingOrganizationalPosition.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.OrganizationalPosition.title && item.Id != request.OrganizationalPosition.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<OrganizationalPositionDTO>.Failure(
                    "سمت سازمانی با این عنوان وجود دارد", 
                    _mapper.Map<OrganizationalPositionDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingOrganizationalPosition.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingOrganizationalPosition.LastModifiedUser = userId;
            }
        }
        


        existingOrganizationalPosition.title = request.OrganizationalPosition.title;
        existingOrganizationalPosition.desc = request.OrganizationalPosition.desc;
        existingOrganizationalPosition.PositionCode = request.OrganizationalPosition.PositionCode;
        
        
        var updatedOrganizationalPosition = _repository.Update(existingOrganizationalPosition);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        //update related constant
        var organizationalPositionConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.OrganizationalPosition.Id);
        var organizationalPositionConstant = await constantRepository.SingleOrDefaultAsync(organizationalPositionConstantQuery, cancellationToken);

        organizationalPositionConstant.title = existingOrganizationalPosition.title;
        organizationalPositionConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                organizationalPositionConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(organizationalPositionConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = organizationalPositionViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingOrganizationalPosition.Id);
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

        return ResultDTO<OrganizationalPositionDTO>.Success(
            _mapper.Map<OrganizationalPositionDTO>(updatedOrganizationalPosition),
            $"سمت سازمانی {updatedOrganizationalPosition.title} با موفقیت بروزرسانی شد"
        );

    }
}
