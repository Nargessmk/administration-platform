using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class VehicleDTOValidator : BaseDTOValidator<VehicleDTO, Guid>
{
    public VehicleDTOValidator()
    {
        // Vehicle-specific validation rules
        
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وسیله نقلیه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);
            
        RuleFor(x => x.VehicleTypeId)
            .NotEmpty()
            .WithMessage("نوع وسیله نقلیه الزامی است");
            
        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("ظرفیت باید بزرگتر از صفر باشد");
            
        RuleFor(x => x.IsActive)
            .NotNull()
            .WithMessage("وضعیت فعال بودن وسیله نقلیه الزامی است");
            
        RuleFor(x => x.VehicleTypeTitle)
            .NotEmpty()
            .MaximumLength(256)
            .WithMessage("عنوان نوع وسیله نقلیه الزامی است و حداکثر 256 کاراکتر");
            
        RuleFor(x => x.LicensePlate)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("پلاک وسیله نقلیه الزامی است و حداکثر 20 کاراکتر");
            
        RuleFor(x => x.ValidityDate)
            .NotEmpty()
            .WithMessage("تاریخ اعتبار الزامی است");
    }
}
