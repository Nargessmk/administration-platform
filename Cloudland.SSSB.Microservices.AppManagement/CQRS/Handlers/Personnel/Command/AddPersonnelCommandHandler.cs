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

public sealed class AddPersonnelCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.SSSB.Models.View.PersonnelView> personnelViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddPersonnelCommand, ResultDTO<PersonnelDTO>?>
{
    public async Task<ResultDTO<PersonnelDTO>?> Handle(AddPersonnelCommand request, CancellationToken cancellationToken)
    {

            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HTTP context is not available");
            }
            
            // Check duplicate (by NationalCode or FirstName+LastName)
            var queryDuplicate = _repository
                .MultipleResultQuery()
                .AndFilter(item => item.FirstName == request.Personnel.FirstName
                                   && item.LastName == request.Personnel.LastName
                                   || item.NationalCode == request.Personnel.NationalCode);
            var duplicate = await _repository.FirstOrDefaultAsync(queryDuplicate, cancellationToken);
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

            // Map DTO to entity
            var newPersonnel = _mapper.Map<Models.Personnel>(request.Personnel);

            newPersonnel.CreateAt = DateTime.Now;
            newPersonnel.LastModifiedAt = DateTime.Now;
        

            if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
            {
                var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
                if (Guid.TryParse(userClaim, out Guid userId))
                {
                    newPersonnel.UserCreated = userId;
                    newPersonnel.LastModifiedUser = userId;
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

            newPersonnel.CustomerId = Guid.NewGuid();
            // Set default values
            newPersonnel.AllowDelete = true;
            newPersonnel.AllowUpdate = true;
            newPersonnel.Disabled = false;
            newPersonnel.hidden = false;

            
            var personnel = await _repository.AddAsync(newPersonnel, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

            // Sync to Elasticsearch from view using repository
            try
            {
                var viewQuery = personnelViewRepository.SingleResultQuery().AndFilter(v => v.Id == newPersonnel.Id);
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
                _mapper.Map<PersonnelDTO>(personnel),
                "پرسنل با موفقیت ایجاد شد"
            );
    }
}
