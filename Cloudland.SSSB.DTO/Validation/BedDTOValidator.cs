using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class BedDTOValidator : BaseDTOValidator<BedDTO, Guid>
{
    public BedDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه تخت باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rules for Required Foreign Keys
        RuleFor(x => x.RoomId)
            .NotEmpty()
            .WithMessage("اتاق باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه اتاق باید یک GUID معتبر باشد");

        RuleFor(x => x.BedStatusId)
            .NotEmpty()
            .WithMessage("وضعیت تخت باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وضعیت تخت باید یک GUID معتبر باشد");

        RuleFor(x => x.BedReservationStatusId)
            .NotEmpty()
            .WithMessage("وضعیت رزرو تخت باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وضعیت رزرو تخت باید یک GUID معتبر باشد");

        // Rule for Optional Foreign Key
        RuleFor(x => x.ReservationId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("شناسه پذیرش باید یک GUID معتبر باشد");

        // Rule for Bed Number
        RuleFor(x => x.BedNumber)
            .NotEmpty()
            .WithMessage("شماره تخت باید وارد شود")
            .MaximumLength(50)
            .WithMessage("شماره تخت نمی‌تواند بیش از 50 کاراکتر باشد");

        // Rule for Capacity
        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("ظرفیت باید بزرگتر از صفر باشد");
    }
}
