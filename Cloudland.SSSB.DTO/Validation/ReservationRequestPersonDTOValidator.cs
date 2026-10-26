using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ReservationRequestPersonDTOValidator : BaseDTOValidator<ReservationRequestPersonDTO, Guid>
{
    public ReservationRequestPersonDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        RuleFor(x => x.PersonnelId)
            .NotEmpty().WithMessage("شناسه پرسنل الزامی است")
            .Must(id => id != null && id != Guid.Empty).WithMessage("شناسه پرسنل باید معتبر باشد");

        RuleFor(x => x.ReservationRequestHeaderId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو غذا الزامی است")
            .Must(id => id != null && id != Guid.Empty).WithMessage("شناسه درخواست رزرو غذا باید معتبر باشد");
    }
}


