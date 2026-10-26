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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.ContractFoodCalendar.Command;

public sealed class AddContractFoodCalendarCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.ContractFoodCalendar> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.ContractFoodCalendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddContractFoodCalendarCommand, ResultDTO<ContractFoodCalendarDTO>?>
{
    public async Task<ResultDTO<ContractFoodCalendarDTO>?> Handle(AddContractFoodCalendarCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var duplicateQuery = _repository
            .SingleResultQuery()
            .AndFilter(item => item.ContractFoodLocationId == request.ContractFoodCalendar.ContractFoodLocationId
                               && item.Date == _mapper.Map<DateTime>(request.ContractFoodCalendar.Date));
        var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
        if (duplicate != null) 
        {
            return ResultDTO<ContractFoodCalendarDTO>.Failure(
                "غذا  با این عنوان در این روز از قبل تعریف شده است", 
                _mapper.Map<ContractFoodCalendarDTO>(duplicate)
            );
        }

        var newEntity = _mapper.Map<Models.ContractFoodCalendar>(request.ContractFoodCalendar);
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

        newEntity.CustomerId = Guid.NewGuid();
        newEntity.AllowDelete = true;
        newEntity.AllowUpdate = true;
        newEntity.Disabled = false;
        newEntity.hidden = false;

        var entity = await _repository.AddAsync(newEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<ContractFoodCalendarDTO>.Success(
            _mapper.Map<ContractFoodCalendarDTO>(entity),
            "تقویم وعده با موفقیت ایجاد شد"
        );
    }
}


