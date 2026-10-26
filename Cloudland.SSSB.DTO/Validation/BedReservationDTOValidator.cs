using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class BedReservationDTOValidator : BaseDTOValidator<ReservationRequestBedDTO, Guid>
{
    public BedReservationDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        RuleFor(x => x.ReservationRequestHeaderId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو غذا الزامی است")
            .Must(id => id != null && id != Guid.Empty).WithMessage("شناسه درخواست رزرو غذا باید معتبر باشد");

        RuleFor(x => x.PersonnelId)
            .NotEmpty().WithMessage("شناسه پرسنل الزامی است")
            .Must(id => id != null && id != Guid.Empty).WithMessage("شناسه پرسنل باید معتبر باشد");

        RuleFor(x => x.ReservationDate)
            .NotEmpty().WithMessage("تاریخ رزرو الزامی است");
    }
}


