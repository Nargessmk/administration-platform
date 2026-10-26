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

public sealed class UpdateContractorCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<ContractorView> contractorViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Contractor, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateContractorCommand, ResultDTO<ContractorDTO>?>
{


    public async Task<ResultDTO<ContractorDTO>?> Handle(UpdateContractorCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Contractor.Id);
        var existingContractor = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingContractor == null)
        {
            return ResultDTO<ContractorDTO>.Failure(
                "پیمانکار پیدا نشد",
                []
            ); 
        }
        
        if (!existingContractor.AllowUpdate)
        {
            return ResultDTO<ContractorDTO>.Failure(
                "پیمانکار قابل بروزرسانی نمی باشد",
                request.Contractor
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.Contractor.title) && request.Contractor.title != existingContractor.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.title == request.Contractor.title && item.Id != request.Contractor.Id);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<ContractorDTO>.Failure(
                    "پیمانکار با این عنوان وجود دارد", 
                    _mapper.Map<ContractorDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }

        existingContractor.LastModifiedAt = DateTime.Now;
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingContractor.LastModifiedUser = userId;
            }
        }
        
        existingContractor.AccountNumber = request.Contractor.AccountNumber;
        existingContractor.Address = request.Contractor.Address;
        existingContractor.FirstName = request.Contractor.FirstName;
        existingContractor.LastName = request.Contractor.LastName;
        existingContractor.MobileNumber = request.Contractor.MobileNumber;
        existingContractor.PhoneNumber = request.Contractor.PhoneNumber;
        existingContractor.PostalCode = request.Contractor.PostalCode;
        existingContractor.IsActive = request.Contractor.IsActive;
        existingContractor.title = request.Contractor.title;
        existingContractor.desc = request.Contractor.desc;
        existingContractor.NationalCode = request.Contractor.NationalCode;
        existingContractor.EconomicNumber = request.Contractor.EconomicNumber;
        existingContractor.RegisterNumber = request.Contractor.RegisterNumber;
        
        
        
        var updatedCompany = _repository.Update(existingContractor);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        //update related constant
        var contractorConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.Contractor.Id);
        var contractorConstant = await constantRepository.SingleOrDefaultAsync(contractorConstantQuery, cancellationToken);

        contractorConstant.title = existingContractor.title;
        contractorConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                contractorConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(contractorConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view
        try
        {
            var viewQuery = contractorViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingContractor.Id);
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
            _mapper.Map<ContractorDTO>(updatedCompany),
            $"پیمانکار {updatedCompany.title} با موفقیت بروزرسانی شد"
        );

    }
}
