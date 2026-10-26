using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Query;

public sealed class GetConstantsByIdsQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<GetConstantsByIdsQueryHandler> logger)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetConstantsByIdsQuery, List<ConstantDTO>>
{
    public async Task<List<ConstantDTO>> Handle(GetConstantsByIdsQuery request, CancellationToken cancellationToken)
    {
        
        if (request.Ids == null || !request.Ids.Any())
        {
            logger.LogWarning("GetConstantsByIdsQuery called with empty or null IDs list");
            return new List<ConstantDTO>();
        }
        
        var validIds = request.Ids.Where(id => id != Guid.Empty).Distinct().ToList();
        
        if (!validIds.Any())
        {
            logger.LogWarning("GetConstantsByIdsQuery called with no valid GUIDs");
            return new List<ConstantDTO>();
        }

        logger.LogDebug("Retrieving {Count} constants for IDs: {Ids}", 
            validIds.Count, string.Join(", ", validIds));

        try
        {
            var query = _repository.MultipleResultQuery()
                .AndFilter(x => validIds.Contains(x.Id))
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.title);

            var constants = await _repository.SearchAsync(query, cancellationToken);
            var result = _mapper.Map<List<ConstantDTO>>(constants);

            logger.LogInformation("Successfully retrieved {Count} constants out of {RequestedCount} requested IDs", 
                result.Count, validIds.Count);
            
            var foundIds = result.Select(c => c.Id).ToHashSet();
            var notFoundIds = validIds.Where(id => !foundIds.Contains(id)).ToList();
            
            if (notFoundIds.Any())
            {
                logger.LogWarning("Constants not found for IDs: {NotFoundIds}", 
                    string.Join(", ", notFoundIds));
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while retrieving constants for IDs: {Ids}", 
                string.Join(", ", validIds));
            throw;
        }
    }
}
