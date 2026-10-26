using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Microservices.AppManagement.CustomResults;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.View;

public sealed class GetPersonnelViewChildrenByParentIdPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<PersonnelView> personnelViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetPersonnelViewChildrenByParentIdPagingQuery, ResultDTO<PaginatedResult<PersonnelViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<PersonnelViewDTO>>> Handle(
        GetPersonnelViewChildrenByParentIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = personnelViewRepository
                .MultipleResultQuery()
                .AndFilter(x => x.ParentId == request.ParentId)
                .OrderByDescending(x => x.CreateAt);

            var entities = await personnelViewRepository.SearchAsync(query, cancellationToken);
            var mappedEntities = entities.Select(x => _mapper.Map<PersonnelViewDTO>(x));

            var paginatedResult = new PaginatedResult<PersonnelViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(mappedEntities);

            return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Success(
                paginatedResult,
                "لیست فرزندان پرسنل با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Failure(
                $"خطا در دریافت لیست فرزندان پرسنل: {ex.Message}",
                []
            );
        }
    }
}
