using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.CQRS.Queries;
using Cloudland.APM.Models.ResultModel;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using System.Globalization;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Calendar;

public sealed class GetCalendarPagingQueryHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Calendar> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Calendar, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<GetCalendarPagingQuery, PagedResult<CalendarDTO>>
{
    private static readonly PersianCalendar PC = new PersianCalendar();

    public async Task<PagedResult<CalendarDTO>> Handle(GetCalendarPagingQuery request,
        CancellationToken cancellationToken)
    {
        // Calculate date range for the Persian year
        DateTime startDate = PC.ToDateTime(request.Year, 1, 1, 0, 0, 0, 0);
        
        // End: Start of next Persian year (exclusive, so we use < instead of <=)
        DateTime nextYearStart = PC.ToDateTime(request.Year + 1, 1, 1, 0, 0, 0, 0);
        
        var totalCount = await _repository.CountAsync(
            x => x.Date >= startDate && x.Date < nextYearStart, 
            cancellationToken);
        
        // Query with year filter
        var query = _repository.MultipleResultQuery()
            .AndFilter(x => x.Date >= startDate && x.Date < nextYearStart)
            .OrderBy(x => x.Date)
            .Page(request.PageNumber, request.PageSize);
            
        var items = await _repository.SearchAsync(query, cancellationToken);
        
        return new PagedResult<CalendarDTO>
        {
            Items = _mapper.Map<List<CalendarDTO>>(items),
            TotalCount = totalCount,
            PageIndex = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

