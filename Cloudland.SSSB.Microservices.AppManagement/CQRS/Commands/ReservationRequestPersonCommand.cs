using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddReservationRequestPersonCommand(ReservationRequestPersonDTO ReservationRequestPerson)
    : AddCommandDifferenceOutput<ReservationRequestPersonDTO, ReservationRequestPersonViewDTO, Guid>(
        ReservationRequestPerson);

public sealed record UpdateReservationRequestPersonCommand(ReservationRequestPersonDTO ReservationRequestPerson) 
    : UpdateCommandDifferenceOutput<ReservationRequestPersonDTO, ReservationRequestPersonViewDTO, Guid>(ReservationRequestPerson);
public sealed record DeleteReservationRequestPersonCommand(Guid Id) : DeleteCommand<Guid>(Id);




