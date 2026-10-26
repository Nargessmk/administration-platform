using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Views;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.AccountNumber.View;

public sealed class GetAccountNumberViewsByOrganizationalChartIdQueryHandler(
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
        IRequestHandler<GetAccountNumberViewsByOrganizationalChartIdQuery, ResultDTO<List<AccountNumberViewDTO>>>
{
    public async Task<ResultDTO<List<AccountNumberViewDTO>>> Handle(
        GetAccountNumberViewsByOrganizationalChartIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = accountNumberViewRepository.MultipleResultQuery()
                .AndFilter(item => item.OrganizationalChartId == request.OrganizationalChartId);

            var entities = await accountNumberViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<AccountNumberViewDTO>>(entities);

            return ResultDTO<List<AccountNumberViewDTO>>.Success(
                dtos,
                "لیست شماره حساب‌ها بر اساس نمودار سازمانی با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<AccountNumberViewDTO>>.Failure(
                $"خطا در دریافت لیست شماره حساب‌ها: {ex.Message}",
                new List<AccountNumberViewDTO>()
            );
        }
    }
}
