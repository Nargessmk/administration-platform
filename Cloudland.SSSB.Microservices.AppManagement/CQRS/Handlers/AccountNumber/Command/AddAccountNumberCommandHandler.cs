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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.AccountNumber.Command;

public sealed class AddAccountNumberCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.AccountNumber> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.AccountNumber, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddAccountNumberCommand, ResultDTO<AccountNumberDTO>?>
{
    public async Task<ResultDTO<AccountNumberDTO>?> Handle(AddAccountNumberCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.accountNumber.AccountNumberValue);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<AccountNumberDTO>.Failure(
                    "شماره حساب با این عنوان از قبل موجود می باشد",
                    _mapper.Map<AccountNumberDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newAccountNumber = _mapper.Map<Models.AccountNumber>(request.accountNumber);

            newAccountNumber.CreateAt = DateTime.Now;
            newAccountNumber.LastModifiedAt = DateTime.Now;

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newAccountNumber.UserCreated = userId;
                    newAccountNumber.LastModifiedUser = userId;
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

            newAccountNumber.CustomerId = Guid.NewGuid();
            // Set default values
            newAccountNumber.AllowDelete = true;
            newAccountNumber.AllowUpdate = true;
            newAccountNumber.Disabled = false;
            newAccountNumber.hidden = false;

            // Add organizational chart to repository
            var organizationalChart = await _repository.AddAsync(newAccountNumber, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            return ResultDTO<AccountNumberDTO>.Success(
                _mapper.Map<AccountNumberDTO>(organizationalChart),
                "شماره حساب با موفقیت ایجاد شد"
            );
    }
}
