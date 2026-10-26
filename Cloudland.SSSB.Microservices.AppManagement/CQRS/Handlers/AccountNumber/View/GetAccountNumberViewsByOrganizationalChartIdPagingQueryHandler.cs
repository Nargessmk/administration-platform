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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.AccountNumber.View;

public sealed class GetAccountNumberViewsByOrganizationalChartIdPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.AccountNumber> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<AccountNumberView> accountNumberViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.AccountNumber, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetAccountNumberViewsByOrganizationalChartIdPagingQuery, ResultDTO<PaginatedResult<AccountNumberViewDTO>>>
{
    public async Task<ResultDTO<PaginatedResult<AccountNumberViewDTO>>> Handle(
        GetAccountNumberViewsByOrganizationalChartIdPagingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = accountNumberViewRepository.MultipleResultQuery()
                .AndFilter(item => item.OrganizationalChartId == request.OrganizationalChartId);

            var entities = await accountNumberViewRepository.SearchAsync(query, cancellationToken);

            var paginatedResult = new PaginatedResult<AccountNumberViewDTO>(request.PageNumber, request.PageSize);
            await paginatedResult.PaginateFromEnumerable(entities.Select(x => _mapper.Map<AccountNumberViewDTO>(x)));

            return ResultDTO<PaginatedResult<AccountNumberViewDTO>>.Success(
                paginatedResult,
                "لیست شماره حساب‌ها بر اساس نمودار سازمانی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PaginatedResult<AccountNumberViewDTO>>.Failure(
                $"خطا در دریافت لیست شماره حساب‌ها: {ex.Message}",
                []
            );
        }
    }
}
