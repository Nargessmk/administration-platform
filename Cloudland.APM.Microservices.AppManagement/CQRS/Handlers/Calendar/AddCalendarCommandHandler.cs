using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DataBaseContext;
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

public sealed class AddCalendarCommandHandler(
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
        IRequestHandler<AddCalendarCommand, CalendarDTO?>
{
    public async Task<CalendarDTO?> Handle(AddCalendarCommand request, CancellationToken cancellationToken)
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

        // Get current user ID
        var userId = new Guid(_httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value);
        var customerId = Guid.NewGuid(); // You may want to get this from context if available

        // Validate NodeTypeId if provided
        Cloudland.APM.Models.Constant? constantNode = null;
        if (request.calendar.NodeTypeId.HasValue)
        {
            var constantNodeQuery = constantRepository.SingleResultQuery()
                .AndFilter(item => item.Id == request.calendar.NodeTypeId.Value);
            constantNode = await constantRepository.SingleOrDefaultAsync(constantNodeQuery, cancellationToken);
            if (constantNode == null)
            {
                throw new Exception($"NodeTypeId {request.calendar.NodeTypeId.Value} not found");
            }
        }

        // Check if calendar entry already exists for this date
        var dateTime = PersianDateTime.Parse(request.calendar.Date).ToDateTime();
        var existingCalendarQuery = calendarRepository
            .SingleResultQuery()
            .AndFilter(item => item.Date.Date == dateTime.Date);
        var existingCalendar = await calendarRepository.SingleOrDefaultAsync(existingCalendarQuery, cancellationToken);
        if (existingCalendar != null)
        {
            return null; // Calendar already exists for this date
        }

        // Create Constant first
        var constantId = Guid.NewGuid();
        var newConstant = new Cloudland.APM.Models.Constant
        {
            Id = constantId,
            ParentId = request.calendar.ParentId,
            title = request.calendar.title ?? request.calendar.Occasion ?? "Calendar Entry",
            NodeTypeId = request.calendar.NodeTypeId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now,
            UserCreated = userId,
            LastModifiedUser = userId,
            CustomerId = customerId,
            ChildMustBeShow = true,
            ConstantValue = "Calendar"
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
            // Default values if NodeTypeId is not provided
            newConstant.AllowDelete = true;
            newConstant.AllowUpdate = true;
            newConstant.Disabled = false;
            newConstant.hidden = false;
        }

        // Create Calendar with the same ID as Constant (one-to-one relationship)
        var calendar = new Cloudland.APM.Models.Calendar
        {
            Id = constantId, // Use Constant's ID
            title = request.calendar.title ?? request.calendar.Occasion ?? "Calendar Entry",
            Date = dateTime,
            Occasion = request.calendar.Occasion,
            AdditionalDescription = request.calendar.AdditionalDescription,
            IsHoliday = request.calendar.IsHoliday,
            IsThursday = request.calendar.IsThursday,
            IsFriday = request.calendar.IsFriday,
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

        // Add both Constant and Calendar
        await constantRepository.AddAsync(newConstant, cancellationToken);
        await calendarRepository.AddAsync(calendar, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch
        try
        {
            var calendarDto = _mapper.Map<CalendarDTO>(calendar);
            await _elasticClient.IndexAsync(calendarDto, i => i
                .Index(ElasticsearchIndexNames.Calendar)
                .Id(calendarDto.Id)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return _mapper.Map<CalendarDTO>(calendar);
    }
}

