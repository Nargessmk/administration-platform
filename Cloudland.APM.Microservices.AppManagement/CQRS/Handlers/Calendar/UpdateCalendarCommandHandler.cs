using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using MD.PersianDateTime.Standard;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Calendar;

public sealed class UpdateCalendarCommandHandler(
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
        IRequestHandler<UpdateCalendarCommand, CalendarDTO>
{
    public async Task<CalendarDTO> Handle(UpdateCalendarCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HttpContext is null");
        }

        var userId = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);

        // Find existing calendar
        var calendarQuery = calendarRepository.SingleResultQuery()
            .AndFilter(item => item.Id == request.calendar.Id);
        var existingCalendar = await calendarRepository.SingleOrDefaultAsync(calendarQuery, cancellationToken);

        if (existingCalendar == null)
        {
            throw new Exception($"Calendar with Id {request.calendar.Id} not found");
        }

        // Parse date if provided
        DateTime dateTime = existingCalendar.Date;
        if (!string.IsNullOrEmpty(request.calendar.Date))
        {
            dateTime = PersianDateTime.Parse(request.calendar.Date).ToDateTime();
        }

        // Update Calendar
        existingCalendar.title = request.calendar.title ?? request.calendar.Occasion ?? existingCalendar.title;
        existingCalendar.Date = dateTime;
        existingCalendar.Occasion = request.calendar.Occasion ?? existingCalendar.Occasion;
        existingCalendar.AdditionalDescription = request.calendar.AdditionalDescription ?? existingCalendar.AdditionalDescription;
        existingCalendar.IsHoliday = request.calendar.IsHoliday;
        existingCalendar.IsThursday = request.calendar.IsThursday;
        existingCalendar.IsFriday = request.calendar.IsFriday;
        existingCalendar.LastModifiedAt = DateTime.Now;
        existingCalendar.LastModifiedUser = userId;

        // Update associated Constant's title
        var constantQuery = constantRepository.SingleResultQuery()
            .AndFilter(item => item.Id == existingCalendar.Id);
        var existingConstant = await constantRepository.SingleOrDefaultAsync(constantQuery, cancellationToken);
        
        if (existingConstant != null)
        {
            existingConstant.title = existingCalendar.title;
            existingConstant.LastModifiedAt = DateTime.Now;
            existingConstant.LastModifiedUser = userId;
            constantRepository.Update(existingConstant);
        }

        calendarRepository.Update(existingCalendar);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch
        try
        {
            var calendarDto = _mapper.Map<CalendarDTO>(existingCalendar);
            await _elasticClient.IndexAsync(calendarDto, i => i
                .Index(ElasticsearchIndexNames.Calendar)
                .Id(calendarDto.Id)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return _mapper.Map<CalendarDTO>(existingCalendar);
    }
}

