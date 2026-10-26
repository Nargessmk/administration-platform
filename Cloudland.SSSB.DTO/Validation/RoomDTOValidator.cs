using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class RoomDTOValidator : BaseDTOValidator<RoomDTO, Guid>
{
    public RoomDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه اتاق باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rule for Required Foreign Keys
        RuleFor(x => x.LocationId)
            .NotEmpty()
            .WithMessage("مکان باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه مکان باید یک GUID معتبر باشد");

        RuleFor(x => x.RoomTypeId)
            .NotEmpty()
            .WithMessage("نوع اتاق باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه نوع اتاق باید یک GUID معتبر باشد");
        
        RuleFor(x => x.RoomStatusId)
            .NotEmpty()
            .WithMessage("وضعیت اتاق باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وضعیت اتاق باید یک GUID معتبر باشد");

        // Rule for Room Number
        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("شماره اتاق باید وارد شود")
            .MaximumLength(50)
            .WithMessage("شماره اتاق نمی‌تواند بیش از 50 کاراکتر باشد");
        

        // Rule for Empty Bed Count
        RuleFor(x => x.EmptyBedCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("تعداد تخت خالی نمی‌تواند منفی باشد");
    }
}
