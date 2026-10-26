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

public sealed class GetPersonnelViewsByAccountNumberIdForReservationRequestPersonnelPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<PersonnelView> personnelViewRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<AccountNumberView> accountNumberViewRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> constantRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetPersonnelViewsByAccountNumberIdForReservationRequestPersonnelPagingQuery, ResultDTO<PaginatedResult<PersonnelViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<PersonnelViewDTO>>> Handle(
        GetPersonnelViewsByAccountNumberIdForReservationRequestPersonnelPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var accountNumberQuery = accountNumberViewRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.AccountNumberId);

            var accountNumberEntity = await accountNumberViewRepository.SingleOrDefaultAsync(accountNumberQuery, cancellationToken);

            var constantQuery = constantRepository.SingleResultQuery()
                .AndFilter(item => item.Id == accountNumberEntity.OrganizationalChartId);

            var constantEntity = await constantRepository.SingleOrDefaultAsync(constantQuery, cancellationToken);

            if (constantEntity == null || !constantEntity.ParentId.HasValue)
            {
                // If no constant entity or no parent ID, return empty result
                var emptyQuery = personnelViewRepository.MultipleResultQuery()
                    .AndFilter(item => false); // This will return no results
                var emptyEntities = await personnelViewRepository.SearchAsync(emptyQuery, cancellationToken);
                
                var emptyPaginatedResult = new PaginatedResult<PersonnelViewDTO>(request.PageNumber, request.PageSize);
                await emptyPaginatedResult.PaginateFromEnumerable(emptyEntities.Select(x => _mapper.Map<PersonnelViewDTO>(x)));
                
                return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Success(
                    emptyPaginatedResult,
                    "لیست پرسنل با موفقیت دریافت شد"
                );
            }

            var parentOrganizationalConstant = constantRepository.MultipleResultQuery()
                .AndFilter(item => item.ParentId == constantEntity.ParentId);
            var parentOrganizationalConstantEntity = await constantRepository.SearchAsync(parentOrganizationalConstant,cancellationToken);
            
            
            // Extract the IDs from the parent organizational constants
            var parentOrganizationalConstantIds = parentOrganizationalConstantEntity
                .Select(c => c.Id)
                .ToList();
            
            var query = personnelViewRepository.MultipleResultQuery()
                .AndFilter(item => item.OrganizationalChartId.HasValue && 
                                   parentOrganizationalConstantIds.Contains(item.OrganizationalChartId.Value) && item.FamilyRelationTypeId == Guid.Parse("c43e971c-c106-4bf7-a749-837715165121"));

            var entities = await personnelViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<PersonnelViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<PersonnelViewDTO>(x)));

            return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Success(
                paginatedResult,
                "لیست پرسنل با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<PersonnelViewDTO>>.Failure(
                $"خطا در دریافت لیست پرسنل: {ex.Message}",
                []
            );
        }
    }
}
