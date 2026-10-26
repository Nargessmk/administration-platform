using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ReservationRequestLetterDTOValidator : BaseDTOValidator<ReservationRequestLetterDTO, Guid>
{
    public ReservationRequestLetterDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        RuleFor(x => x.ReservationRequestHeaderId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو غذا الزامی است")
            .Must(id => id != null && id != Guid.Empty).WithMessage("شناسه درخواست رزرو غذا باید معتبر باشد");

        RuleFor(x => x.LetterDate)
            .NotEmpty().WithMessage("تاریخ نامه الزامی است");

        RuleFor(x => x.LetterNumber)
            .NotEmpty().WithMessage("شماره نامه الزامی است");

        RuleFor(x => x.LetterSubject)
            .NotEmpty().WithMessage("موضوع نامه الزامی است");
    }
}


