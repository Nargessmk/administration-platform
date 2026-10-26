using System.Text.Json;
using AutoMapper;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Microservices.AppManagement.Services;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;


public sealed class RetrieveJsonDataQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> _repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    IDtoTypeService dtoTypeService)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, _repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<RetrieveJsonDataQuery , object?>
{
    public async Task<object?> Handle(RetrieveJsonDataQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.NodeType))
            throw new ArgumentException("Node type cannot be null or empty.", nameof(request));
        
        var dtoTypeName = dtoTypeService.ConvertToDtoTypeName(request.NodeType);

        if (!dtoTypeService.DtoTypeMap.TryGetValue(dtoTypeName, out var dtoType))
            throw new NotSupportedException($"The object type '{request.NodeType}' is not supported.");

        var query = _repository.SingleResultQuery()
            .AndFilter(item => item.Id == request.NodeId);
        var constant = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        if (constant == null )
            return null;
        
        if(string.IsNullOrEmpty(constant.JsonData))
            return  Activator.CreateInstance(dtoType);
        
        try
        {
            var result = JsonSerializer.Deserialize(
                constant.JsonData, 
                dtoType, 
                dtoTypeService.JsonOptions);
            
            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON data for node type {request.NodeType}", ex);
        }
    }
    
}