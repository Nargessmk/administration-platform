using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.SSSB.DataBaseContext;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Personnel.Query;

public sealed class GetPersonnelByIdQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Personnel, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetPersonnelByIdQuery, ResultDTO<PersonnelDTO>>
{
    public async Task<ResultDTO<PersonnelDTO>> Handle(GetPersonnelByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var _dbContext = _unitOfWork.DbContext as SSSBDBContext 
                ?? throw new InvalidOperationException("SSSBDBContext not available in unit of work");

            var personnel = await _dbContext.Personnels
                .Where(x => x.Id == request.Id)
                .Where(x => !x.Disabled)
                .Where(x => !x.hidden)   
                .FirstOrDefaultAsync(cancellationToken);

            if (personnel == null)
            {
                return ResultDTO<PersonnelDTO>.Failure(
                    "پرسنل مورد نظر یافت نشد ",
                    []
                );
            }

            return ResultDTO<PersonnelDTO>.Success(
                _mapper.Map<PersonnelDTO>(personnel),
                "پرسنل با موفقیت برگشت داده شد"
            );
        }
        catch (Exception ex)
        {
            return ResultDTO<PersonnelDTO>.Failure(
                $"خطا در دریافت پرسنل: {ex.Message}",
                []
            );
        }
    }
}
