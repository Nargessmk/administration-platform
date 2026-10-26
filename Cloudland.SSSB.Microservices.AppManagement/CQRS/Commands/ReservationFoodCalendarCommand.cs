using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddReservationFoodCalendarCommand(ReservationFoodCalendarDTO ReservationFoodCalendar)
    : AddCommandDifferenceOutput<ReservationFoodCalendarDTO, ReservationFoodCalendarViewDTO, Guid>(
        ReservationFoodCalendar);

public sealed record UpdateReservationFoodCalendarCommand(ReservationFoodCalendarDTO ReservationFoodCalendar) 
    : UpdateCommandDifferenceOutput<ReservationFoodCalendarDTO, ReservationFoodCalendarViewDTO, Guid>(ReservationFoodCalendar);
public sealed record DeleteReservationFoodCalendarCommand(Guid Id) : DeleteCommand<Guid>(Id);

public sealed record BulkCreateFoodReservationCommand(List<ShiftWorkerRequestDTO> Requests)
    : IRequest<ResultDTO<int>>;
