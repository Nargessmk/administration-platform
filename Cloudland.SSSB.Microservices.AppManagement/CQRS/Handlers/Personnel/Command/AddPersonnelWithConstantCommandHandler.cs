using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.Command;

public sealed class AddPersonnelWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<PersonnelView> personnelViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddPersonnelWithConstantCommand, ResultDTO<PersonnelDTO>?>
{
    public async Task<ResultDTO<PersonnelDTO>?> Handle(AddPersonnelWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        // Check duplicate (by NationalCode or FirstName+LastName)
        var duplicateQuery = _repository
            .MultipleResultQuery()
            .AndFilter(item => (item.FirstName == request.Personnel.FirstName && item.LastName == request.Personnel.LastName)
                               || item.NationalCode == request.Personnel.NationalCode);
        var duplicate = await _repository.FirstOrDefaultAsync(duplicateQuery, cancellationToken);
        if (duplicate != null)
        {
            var message = duplicate.NationalCode == request.Personnel.NationalCode
                ? "پرسنل با این کد ملی از قبل در سیستم ثبت شده است (تکراری)"
                : "پرسنل با این نام و نام خانوادگی از قبل در سیستم ثبت شده است (تکراری)";

            return ResultDTO<PersonnelDTO>.Failure(
                message,
                _mapper.Map<PersonnelDTO>(duplicate)
            );
        }

        // 1) Create Constant with ParentId from route
        var newConstant = new APM.Models.Constant
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now
        };

        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid constantUserId))
            {
                newConstant.UserCreated = constantUserId;
                newConstant.LastModifiedUser = constantUserId;
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

        newConstant.CustomerId = Guid.NewGuid();
        newConstant.AllowDelete = true;
        newConstant.AllowUpdate = true;
        newConstant.Disabled = false;
        newConstant.hidden = false;
        newConstant.title = $"{request.Personnel.FirstName +' '+ request.Personnel.LastName +'-'+ request.Personnel.NationalCode +'-'+ request.Personnel.PersonnelCode}";
        newConstant.desc = request.Personnel.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("215b9fc6-7fe3-4663-b192-aabf4cc73e1e");
        newConstant.ConstantValue = "Personnel";
        
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        
        var personnelEntity = _mapper.Map<Models.Personnel>(request.Personnel);
        personnelEntity.Id = addedConstant.Id;
        personnelEntity.CreateAt = DateTime.Now;
        personnelEntity.LastModifiedAt = DateTime.Now;

        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            if (Guid.TryParse(userClaim, out Guid userId))
            {
                personnelEntity.UserCreated = userId;
                personnelEntity.LastModifiedUser = userId;
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

        personnelEntity.CustomerId = Guid.NewGuid();
        personnelEntity.AllowDelete = true;
        personnelEntity.AllowUpdate = true;
        personnelEntity.Disabled = false;
        personnelEntity.hidden = false;

        var added = await _repository.AddAsync(personnelEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = personnelViewRepository.SingleResultQuery().AndFilter(v => v.Id == personnelEntity.Id);
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
            _mapper.Map<PersonnelDTO>(added),
            "پرسنل با موفقیت ایجاد شد"
        );
    }
}


