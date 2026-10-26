using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Calendar;

public sealed class DeleteCalendarCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Calendar> calendarRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> constantRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Calendar, Guid>(unitOfWork, calendarRepository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteCalendarCommand, int>
{
    public async Task<int> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken)
    {
        // Find existing calendar
        var calendarQuery = calendarRepository.SingleResultQuery()
            .AndFilter(item => item.Id == request.Id);
        var existingCalendar = await calendarRepository.SingleOrDefaultAsync(calendarQuery, cancellationToken);

        if (existingCalendar == null)
        {
            throw new Exception($"Calendar with Id {request.Id} not found");
        }

        // Delete Calendar first
        var effectedRows = await calendarRepository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        
        // Then delete the associated Constant
        var constantQuery = constantRepository.SingleResultQuery()
            .AndFilter(item => item.Id == request.Id);
        var existingConstant = await constantRepository.SingleOrDefaultAsync(constantQuery, cancellationToken);
        
        if (existingConstant != null)
        {
            await constantRepository.RemoveAsync(x => x.Id == request.Id, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch - Delete from index
        try
        {
            await _elasticClient.DeleteAsync<Cloudland.APM.DTO.CalendarDTO>(request.Id, i => i
                .Index(ElasticsearchIndexNames.Calendar)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return effectedRows;
    }
}

