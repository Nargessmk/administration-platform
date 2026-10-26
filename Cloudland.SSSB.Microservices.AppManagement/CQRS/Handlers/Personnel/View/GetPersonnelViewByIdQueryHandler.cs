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

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.View;

public sealed class GetPersonnelViewByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<PersonnelView> personnelViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetPersonnelViewByIdQuery, ResultDTO<PersonnelViewDTO>>
{
    public async Task<ResultDTO<PersonnelViewDTO>> Handle(
        GetPersonnelViewByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = personnelViewRepository
                .SingleResultQuery()
                .AndFilter(x => x.Id == request.Id && !x.Disabled && !x.hidden);

            var entity = await personnelViewRepository.SingleOrDefaultAsync(query, cancellationToken);

            if (entity is null)
            {
                return ResultDTO<PersonnelViewDTO>.Failure(
                    "پرسنل مورد نظر یافت نشد",
                    []
                );
            }

            var dto = _mapper.Map<PersonnelViewDTO>(entity);

            return ResultDTO<PersonnelViewDTO>.Success(
                dto,
                "پرسنل با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PersonnelViewDTO>.Failure(
                $"خطا در دریافت اطلاعات پرسنل: {ex.Message}",
                []
            );
        }
    }
}
