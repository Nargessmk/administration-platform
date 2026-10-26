using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;


public class LocationDTOValidator : BaseDTOValidator<LocationDTO, Guid>
{
    public LocationDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه مکان باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rule for Address
        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("آدرس نمی‌تواند بیش از 500 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        // Rule for Phone Number
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .WithMessage("شماره تلفن نمی‌تواند بیش از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        // Rule for Room Count
        RuleFor(x => x.RoomCount)
            .NotEmpty()
            .WithMessage("تعداد اتاق نباید خالی باشد");

        // Rule for Required Foreign Key
        RuleFor(x => x.LocationTypeId)
            .NotEmpty()
            .WithMessage("نوع مکان باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه نوع مکان باید یک GUID معتبر باشد");
    }
}