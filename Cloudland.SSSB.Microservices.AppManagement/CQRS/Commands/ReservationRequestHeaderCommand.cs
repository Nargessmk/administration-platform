using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddReservationRequestHeaderCommand(ReservationRequestHeaderDTO ReservationRequestHeader)
    : AddCommandDifferenceOutput<ReservationRequestHeaderDTO, ReservationRequestHeaderViewDTO, Guid>(
        ReservationRequestHeader);

public sealed record UpdateReservationRequestHeaderCommand(ReservationRequestHeaderDTO ReservationRequestHeader) 
    : UpdateCommandDifferenceOutput<ReservationRequestHeaderDTO, ReservationRequestHeaderViewDTO, Guid>(ReservationRequestHeader);
public sealed record DeleteReservationRequestHeaderCommand(Guid Id) : DeleteCommand<Guid>(Id);



