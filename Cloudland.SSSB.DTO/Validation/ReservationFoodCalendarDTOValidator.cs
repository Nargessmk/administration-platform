using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ReservationFoodCalendarDTOValidator : BaseDTOValidator<ReservationFoodCalendarDTO, Guid>
{
    public ReservationFoodCalendarDTOValidator()
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

        RuleFor(x => x.ContractFoodCalendarId)
            .NotEmpty().WithMessage("شناسه تقویم غذای قرارداد الزامی است")
            .Must(id => id != null && id != Guid.Empty).WithMessage("شناسه تقویم غذای قرارداد باید معتبر باشد");

        RuleFor(x => x.ReservationDate)
            .NotEmpty().WithMessage("تاریخ رزرو الزامی است");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بیشتر از صفر باشد");
    }
}


