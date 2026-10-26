using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands.Base;
using MediatR;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Commands;

public sealed record GenerateCalendarCommand(int Year, Guid ParentId, Guid? NodeTypeId = null) : IRequest<int>;

public sealed record AddCalendarCommand(CalendarDTO calendar) : AddCommand<CalendarDTO?>(calendar);

public sealed record UpdateCalendarCommand(CalendarDTO calendar) : UpdateCommand<CalendarDTO>(calendar);

public sealed record DeleteCalendarCommand(Guid Id) : DeleteCommand<Guid>(Id);

