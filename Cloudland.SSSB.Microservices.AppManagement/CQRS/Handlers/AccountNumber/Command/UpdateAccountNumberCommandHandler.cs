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

public sealed class UpdateAccountNumberCommandHandler(
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
        IRequestHandler<UpdateAccountNumberCommand, ResultDTO<AccountNumberDTO>?>
{


    public async Task<ResultDTO<AccountNumberDTO>?> Handle(UpdateAccountNumberCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.accountNumber.Id);
        var existingAccountNumber = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingAccountNumber == null)
        {
            return ResultDTO<AccountNumberDTO>.Failure(
                "شماره حساب پیدا نشد",
                []
            ); 
        }
        
        if (!existingAccountNumber.AllowUpdate)
        {
            return ResultDTO<AccountNumberDTO>.Failure(
                "شماره حساب قابل بروزرسانی نمی باشد",
                request.accountNumber
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.accountNumber.title) && request.accountNumber.title != existingAccountNumber.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.accountNumber.title && item.Id != request.accountNumber.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<AccountNumberDTO>.Failure(
                    "شماره حساب با این عنوان وجود دارد", 
                    _mapper.Map<AccountNumberDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingAccountNumber.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingAccountNumber.LastModifiedUser = userId;
            }
        }
        
        existingAccountNumber.AccountNumberValue = request.accountNumber.AccountNumberValue;
        existingAccountNumber.title = request.accountNumber.title;
        existingAccountNumber.desc = request.accountNumber.desc;
        existingAccountNumber.OrganizationalChartId = request.accountNumber.OrganizationalChartId;
        
        
        
        var updatedCompany = _repository.Update(existingAccountNumber);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<AccountNumberDTO>.Success(
            _mapper.Map<AccountNumberDTO>(updatedCompany),
            $"شماره حساب {updatedCompany.title} با موفقیت بروزرسانی شد"
        );

    }
}
