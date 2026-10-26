using AutoMapper;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contract.Command;

public sealed class AddContractCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contract> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contract, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddContractCommand, ResultDTO<ContractDTO>?>
{
    public async Task<ResultDTO<ContractDTO>?> Handle(AddContractCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var dbContext = _unitOfWork.DbContext as SSSBDBContext;
        if (dbContext == null)
        {
            throw new Exception("SSSB database context is not available");
        }

        // Uniqueness: ContractNumber per Contractor active period
        if (!string.IsNullOrEmpty(request.Contract.ContractNumber) && request.Contract.ParentId == null)
        {
            var duplicate = await dbContext.Contracts
                .Where(x => !x.Disabled && !x.hidden)
                .Where(x => x.ContractorId == request.Contract.ContractorId)
                .Where(x => x.ContractNumber == request.Contract.ContractNumber)
                .FirstOrDefaultAsync(cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<ContractDTO>.Failure(
                    "قراردادی با این شماره برای این پیمانکار وجود دارد",
                    _mapper.Map<ContractDTO>(duplicate)
                );
            }
        }

        var newEntity = _mapper.Map<Models.Contract>(request.Contract);

        newEntity.CreateAt = DateTime.Now;
        newEntity.LastModifiedAt = DateTime.Now;

        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                newEntity.UserCreated = userId;
                newEntity.LastModifiedUser = userId;
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

        newEntity.Id = Guid.NewGuid();
        newEntity.CustomerId = Guid.NewGuid();
        newEntity.AllowDelete = true;
        newEntity.AllowUpdate = true;
        newEntity.Disabled = false;
        newEntity.hidden = false;
        
        var added = await _repository.AddAsync(newEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<ContractDTO>.Success(
            _mapper.Map<ContractDTO>(added),
            "قرارداد با موفقیت ایجاد شد"
        );
    }
}


