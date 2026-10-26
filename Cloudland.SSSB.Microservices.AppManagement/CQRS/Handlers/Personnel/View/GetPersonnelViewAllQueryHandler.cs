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

public sealed class GetPersonnelViewAllQueryHandler(
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
        IRequestHandler<GetPersonnelViewAllQuery, ResultDTO<List<PersonnelViewDTO>>>
{
    public async Task<ResultDTO<List<PersonnelViewDTO>>> Handle(
        GetPersonnelViewAllQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = personnelViewRepository.MultipleResultQuery();

            var entities = await personnelViewRepository.SearchAsync(query, cancellationToken);

            var dtos = _mapper.Map<List<PersonnelViewDTO>>(entities);

            return ResultDTO<List<PersonnelViewDTO>>.Success(
                dtos,
                "لیست کامل پرسنل با موفقیت دریافت شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<List<PersonnelViewDTO>>.Failure(
                $"خطا در دریافت لیست پرسنل: {ex.Message}",
                new List<PersonnelViewDTO>()
            );
        }
    }
}
