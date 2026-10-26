using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DataBaseContext;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using Cloudland.APM.Utils.Calendar;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;
using MD.PersianDateTime.Standard;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Calendar;

public sealed class GenerateCalendarCommandHandler(
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
        IRequestHandler<GenerateCalendarCommand, int>
{
    public async Task<int> Handle(GenerateCalendarCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HttpContext is null");
        }

        var apmDbContext = _unitOfWork.DbContext as APMDBContext;
        if (apmDbContext == null)
        {
            throw new Exception("DbContext is not APMDBContext");
        }
        
        var dayInfos = PersianCalendarGenerator.GetYearDays(request.Year);

        // Get current user ID
        var userId = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);
        var customerId = Guid.NewGuid(); 

        Cloudland.APM.Models.Constant? constantNode = null;
        if (request.NodeTypeId.HasValue)
        {
            var constantNodeQuery = constantRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.NodeTypeId.Value);
            constantNode = await constantRepository.SingleOrDefaultAsync(constantNodeQuery, cancellationToken);
            if (constantNode == null)
            {
                throw new Exception($"NodeTypeId {request.NodeTypeId.Value} not found");
            }
        }

        var newCalendars = new List<Cloudland.APM.Models.Calendar>();
        var updatedCalendars = new List<Cloudland.APM.Models.Calendar>();
        
        foreach (var dayInfo in dayInfos)
        {
            var dateTime = PersianDateTime.Parse(dayInfo.Date).ToDateTime();

            var existingCalendarQuery = _repository
                .SingleResultQuery()
                .AndFilter(item => item.Date.Date == dateTime.Date);
            var existingCalendar = await _repository.SingleOrDefaultAsync(existingCalendarQuery, cancellationToken);

            if (existingCalendar != null)
            {
                // Update existing calendar entry
                existingCalendar.title = dayInfo.Title;
                existingCalendar.Occasion = dayInfo.Occasion;
                existingCalendar.IsHoliday = dayInfo.IsHoliday;
                existingCalendar.IsThursday = dayInfo.IsThursday;
                existingCalendar.IsFriday = dayInfo.IsFriday;
                existingCalendar.LastModifiedAt = DateTime.Now;
                existingCalendar.LastModifiedUser = userId;
                
                var existingConstantQuery = constantRepository.SingleResultQuery()
                    .AndFilter(item => item.Id == existingCalendar.Id);
                var existingConstant = await constantRepository.SingleOrDefaultAsync(existingConstantQuery, cancellationToken);
                if (existingConstant != null)
                {
                    existingConstant.title = dayInfo.Title;
                    existingConstant.LastModifiedAt = DateTime.Now;
                    existingConstant.LastModifiedUser = userId;
                    constantRepository.Update(existingConstant);
                }
                
                _repository.Update(existingCalendar);
                updatedCalendars.Add(existingCalendar);
            }
            else
            {
                var constantId = Guid.NewGuid();
                var newConstant = new Cloudland.APM.Models.Constant
                {
                    Id = constantId,
                    ParentId = request.ParentId,
                    title = dayInfo.Title,
                    NodeTypeId = request.NodeTypeId,
                    CreateAt = DateTime.Now,
                    LastModifiedAt = DateTime.Now,
                    UserCreated = userId,
                    LastModifiedUser = userId,
                    CustomerId = customerId,
                    ChildMustBeShow = true,
                    ConstantValue = "Calendar",
                    desc = dayInfo.Occasion
                };

                // Set properties from constantNode if available
                if (constantNode != null)
                {
                    newConstant.AllowDelete = constantNode.AllowDelete;
                    newConstant.AllowUpdate = constantNode.AllowUpdate;
                    newConstant.Disabled = constantNode.Disabled;
                    newConstant.hidden = constantNode.hidden;
                }
                else
                {
                    newConstant.AllowDelete = true;
                    newConstant.AllowUpdate = true;
                    newConstant.Disabled = false;
                    newConstant.hidden = false;
                }

                var calendar = new Cloudland.APM.Models.Calendar
                {
                    Id = constantId, 
                    title = dayInfo.Title,
                    Date = dateTime,
                    Occasion = dayInfo.Occasion,
                    IsHoliday = dayInfo.IsHoliday,
                    IsThursday = dayInfo.IsThursday,
                    IsFriday = dayInfo.IsFriday,
                    CreateAt = DateTime.Now,
                    LastModifiedAt = DateTime.Now,
                    UserCreated = userId,
                    LastModifiedUser = userId,
                    CustomerId = customerId,
                    AllowDelete = newConstant.AllowDelete,
                    AllowUpdate = newConstant.AllowUpdate,
                    Disabled = newConstant.Disabled,
                    hidden = newConstant.hidden
                };

                await constantRepository.AddAsync(newConstant, cancellationToken);
                newCalendars.Add(calendar);
            }
        }

        if (newCalendars.Any())
        {
            await _repository.AddRangeAsync(newCalendars, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch
        try
        {
            var allCalendarsToSync = newCalendars.Concat(updatedCalendars).ToList();
            foreach (var calendar in allCalendarsToSync)
            {
                var calendarDto = _mapper.Map<Cloudland.APM.DTO.CalendarDTO>(calendar);
                await _elasticClient.IndexAsync(calendarDto, i => i
                    .Index(ElasticsearchIndexNames.Calendar)
                    .Id(calendarDto.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return newCalendars.Count;
    }
}

