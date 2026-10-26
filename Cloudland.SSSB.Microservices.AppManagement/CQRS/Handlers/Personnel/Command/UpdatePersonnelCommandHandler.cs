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
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.Command;

public sealed class UpdatePersonnelCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.SSSB.Models.View.PersonnelView> personnelViewRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdatePersonnelCommand, ResultDTO<PersonnelDTO>?>
{


    public async Task<ResultDTO<PersonnelDTO>?> Handle(UpdatePersonnelCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }
        
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Personnel.Id);
        var existingPersonnel = await _repository.SingleOrDefaultAsync(query, cancellationToken);
        
        if (existingPersonnel == null)
        {
            return ResultDTO<PersonnelDTO>.Failure(
                "پرسنل پیدا نشد",
                []
            ); 
        }
        
        if (!existingPersonnel.AllowUpdate)
        {
            return ResultDTO<PersonnelDTO>.Failure(
                "پرسنل قابل بروزرسانی نمی باشد",
                request.Personnel
            ); 
        }

        // Check for duplicate title 
        if (!string.IsNullOrEmpty(request.Personnel.title) && request.Personnel.title != existingPersonnel.title)
        {
            var duplicateQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.FirstName == request.Personnel.FirstName
                                   && item.LastName == request.Personnel.LastName
                                   || item.NationalCode == request.Personnel.NationalCode);
            var duplicate = await _repository.SingleOrDefaultAsync(duplicateQuery, cancellationToken);
            if (duplicate != null) 
            {
                return ResultDTO<PersonnelDTO>.Failure(
                    "پرسنل با این عنوان وجود دارد", 
                    _mapper.Map<PersonnelDTO>(duplicate) // Optionally return the duplicate
                );
            }
        }
        
        // Get user ID from claims
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                existingPersonnel.LastModifiedUser = userId;
            }
        }
        
        existingPersonnel.desc = request.Personnel.desc;
        existingPersonnel.FirstName = request.Personnel.FirstName;
        existingPersonnel.LastName = request.Personnel.LastName;
        existingPersonnel.NationalCode = request.Personnel.NationalCode;
        existingPersonnel.PersonnelCode = request.Personnel.PersonnelCode;
        existingPersonnel.FamilyRelationTypeId = request.Personnel.FamilyRelationTypeId;
        existingPersonnel.DateOfBirth = _mapper.Map<DateTime?>(request.Personnel.DateOfBirth);
        existingPersonnel.PhoneNumber = request.Personnel.PhoneNumber;
        existingPersonnel.Mobile1 = request.Personnel.Mobile1;
        existingPersonnel.Mobile2 = request.Personnel.Mobile2;
        existingPersonnel.HasShift = request.Personnel.HasShift;
        existingPersonnel.IsDailyWorker = request.Personnel.IsDailyWorker;
        existingPersonnel.HasFoodReservation = request.Personnel.HasFoodReservation;
        existingPersonnel.ContractTypeId = request.Personnel.ContractTypeId;
        existingPersonnel.IsActive = request.Personnel.IsActive;
        existingPersonnel.IsRetired = request.Personnel.IsRetired;
        existingPersonnel.DisplayName = request.Personnel.DisplayName;
        existingPersonnel.Address = request.Personnel.Address;
        existingPersonnel.OrganizationalPositionId = request.Personnel.OrganizationalPositionId;
        existingPersonnel.OrganizationalChartId = request.Personnel.OrganizationalChartId;
        existingPersonnel.Signature = request.Personnel.Signature;
        existingPersonnel.WalletBalance = request.Personnel.WalletBalance;
        existingPersonnel.IsSupervisor = request.Personnel.IsSupervisor;
        existingPersonnel.LastModifiedAt = DateTime.Now;
        existingPersonnel.FusionUserId = request.Personnel.FusionUserId;
        
        
        var updatedPersonnel = _repository.Update(existingPersonnel);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        
        //update related constant
        var personnelConstantQuery = constantRepository.SingleResultQuery().AndFilter(item => item.Id == request.Personnel.Id);
        var personnelConstant = await constantRepository.SingleOrDefaultAsync(personnelConstantQuery, cancellationToken);

        personnelConstant.title = $"{request.Personnel.FirstName +' '+ request.Personnel.LastName +'-'+ request.Personnel.NationalCode +'-'+ request.Personnel.PersonnelCode}";
        personnelConstant.LastModifiedAt = DateTime.Now;
        
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                personnelConstant.LastModifiedUser = userId;
            }
        }
        constantRepository.Update(personnelConstant);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = personnelViewRepository.SingleResultQuery().AndFilter(v => v.Id == existingPersonnel.Id);
            var viewRow = await personnelViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Personnel)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<PersonnelDTO>.Success(
            _mapper.Map<PersonnelDTO>(updatedPersonnel),
            $"پرسنل {updatedPersonnel.title} با موفقیت بروزرسانی شد"
        );

    }
}
