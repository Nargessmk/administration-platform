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
using Cloudland.SSSB.Models.View;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Contractor.Command;

public sealed class AddContractorCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractorView> contractorViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddContractorCommand, ResultDTO<ContractorDTO>?>
{
    public async Task<ResultDTO<ContractorDTO>?> Handle(AddContractorCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }

            // Check for duplicate title
            var queryDuplicate = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.Contractor.title);
            var duplicate = await _repository.SingleOrDefaultAsync(queryDuplicate, cancellationToken);
            if (duplicate != null)
            {
                return ResultDTO<ContractorDTO>.Failure(
                    "پیمانکار با این عنوان از قبل موجود می باشد",
                    _mapper.Map<ContractorDTO>(duplicate)
                );
            }

            // Map DTO to entity
            var newContractor = _mapper.Map<Models.Contractor>(request.Contractor);

            newContractor.CreateAt = DateTime.Now;
            newContractor.LastModifiedAt = DateTime.Now;

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newContractor.UserCreated = userId;
                    newContractor.LastModifiedUser = userId;
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

            newContractor.CustomerId = Guid.NewGuid();
            // Set default values
            newContractor.AllowDelete = true;
            newContractor.AllowUpdate = true;
            newContractor.Disabled = false;
            newContractor.hidden = false;

            // Add organizational chart to repository
            var contractor = await _repository.AddAsync(newContractor, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view
            try
            {
                var viewQuery = contractorViewRepository.SingleResultQuery().AndFilter(v => v.Id == newContractor.Id);
                var viewRow = await contractorViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
                  
                if (viewRow != null)
                {
                    await _elasticClient.IndexAsync(viewRow, i => i
                            .Index(ElasticsearchIndexNames.Contractor)
                            .Id(viewRow.Id)
                        , cancellationToken);
                }
            }
            catch
            {
                // ignore ES sync errors for now; logging can be added later
            }

            return ResultDTO<ContractorDTO>.Success(
                _mapper.Map<ContractorDTO>(contractor),
                "پیمانکار با موفقیت ایجاد شد"
            );
    }
}
