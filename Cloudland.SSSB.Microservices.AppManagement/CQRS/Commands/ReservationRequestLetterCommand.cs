using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddReservationRequestLetterCommand(ReservationRequestLetterDTO ReservationRequestLetter) : AddCommand<ReservationRequestLetterDTO>(ReservationRequestLetter);
public sealed record UpdateReservationRequestLetterCommand(ReservationRequestLetterDTO ReservationRequestLetter) : UpdateCommand<ReservationRequestLetterDTO>(ReservationRequestLetter);
public sealed record DeleteReservationRequestLetterCommand(Guid Id) : DeleteCommand<Guid>(Id);




