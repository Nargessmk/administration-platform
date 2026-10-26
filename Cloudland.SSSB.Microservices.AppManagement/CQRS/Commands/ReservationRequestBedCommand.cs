using Cloudland.APM.DTO;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.DTO.ViewDTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddReservationRequestBedCommand(ReservationRequestBedDTO ReservationRequestBed)
    : AddCommandDifferenceOutput<ReservationRequestBedDTO, ReservationRequestBedViewDTO, Guid>(
        ReservationRequestBed);

public sealed record UpdateReservationRequestBedCommand(ReservationRequestBedDTO ReservationRequestBed) 
    : UpdateCommandDifferenceOutput<ReservationRequestBedDTO, ReservationRequestBedViewDTO, Guid>(ReservationRequestBed);
public sealed record DeleteReservationRequestBedCommand(Guid Id) : DeleteCommand<Guid>(Id);




