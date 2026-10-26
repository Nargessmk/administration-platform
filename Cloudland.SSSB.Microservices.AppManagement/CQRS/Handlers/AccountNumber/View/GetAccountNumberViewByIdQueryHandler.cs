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

public sealed class GetAccountNumberViewByIdQueryHandler(
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
        IRequestHandler<GetAccountNumberViewByIdQuery, ResultDTO<AccountNumberViewDTO>>
{
    public async Task<ResultDTO<AccountNumberViewDTO>> Handle(
        GetAccountNumberViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = accountNumberViewRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.Id);

            var entity = await accountNumberViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity == null)
            {
                return ResultDTO<AccountNumberViewDTO>.Failure(
                    "شماره حساب مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<AccountNumberViewDTO>(entity);

            return ResultDTO<AccountNumberViewDTO>.Success(
                dto,
                "شماره حساب با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<AccountNumberViewDTO>.Failure(
                $"خطا در دریافت شماره حساب: {ex.Message}",
                []
            );
        }
    }
}
