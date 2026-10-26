using AutoMapper;
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
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Organizational_Charts.Command;

public sealed class AddOrganizationalChartCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChart> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<OrganizationalChartView> organizationalChartViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<OrganizationalChart, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddOrganizationalChartCommand, ResultDTO<OrganizationalChartDTO>?>
{


    public async Task<ResultDTO<OrganizationalChartDTO>?> Handle(AddOrganizationalChartCommand request, CancellationToken cancellationToken)
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

            // Map DTO to entity
            var newOrganizationalChart = _mapper.Map<OrganizationalChart>(request.organizationalChart);

            newOrganizationalChart.CreateAt = DateTime.Now;
            newOrganizationalChart.LastModifiedAt = DateTime.Now;

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newOrganizationalChart.UserCreated = userId;
                    newOrganizationalChart.LastModifiedUser = userId;
                }
                else
                {
                    throw new Exception("Invalid user ID in claims");
                }
            }
            else
            {
                throw new Exception("User ID claim not found");
            }

            newOrganizationalChart.CustomerId = Guid.NewGuid();
            // Set default values
            newOrganizationalChart.AllowDelete = true;
            newOrganizationalChart.AllowUpdate = true;
            newOrganizationalChart.Disabled = false;
            newOrganizationalChart.hidden = false;

            // Add organizational chart to repository
            var organizationalChart = await _repository.AddAsync(newOrganizationalChart, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view using repository
            try
            {
                var viewQuery = organizationalChartViewRepository.SingleResultQuery().AndFilter(v => v.Id == newOrganizationalChart.Id);
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
                _mapper.Map<OrganizationalChartDTO>(organizationalChart),
                "سازمان با موفقیت ایجاد شد"
            );
    }
}
