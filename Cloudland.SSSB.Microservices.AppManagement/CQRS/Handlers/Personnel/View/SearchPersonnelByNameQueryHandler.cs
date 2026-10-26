using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.View;

public sealed class SearchPersonnelByNameQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<PersonnelView> personnelViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(
        unitOfWork, null!, mapper, publishEndpoint, elasticClient,
        distributedCache, httpContextAccessor, dbSession),
      IRequestHandler<SearchPersonnelByNameQuery, ResultDTO<PaginatedResult<PersonnelViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<PersonnelViewDTO>>> Handle(
        SearchPersonnelByNameQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Base query
            var query = personnelViewRepository.MultipleResultQuery();

            // Search in both FirstName and LastName (case-insensitive)
            query = query.AndFilter(x =>
                EF.Functions.Like(x.FirstName!, $"%{request.SearchTerm}%") ||
                EF.Functions.Like(x.LastName!, $"%{request.SearchTerm}%"));

            // Get paginated results
            var entities = await personnelViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<PersonnelViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<PersonnelViewDTO>(x)));

            return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Success(
                paginatedResult,
                "جستجوی پرسنل با موفقیت انجام شد."
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Failure(
                $"خطا در جستجوی لیست پرسنل: {ex.Message}",
                []
            );
        }
    }
}
