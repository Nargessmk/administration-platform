using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class VehicleDriverDTOValidator : BaseDTOValidator<VehicleDriverDTO, Guid>
{
    public VehicleDriverDTOValidator()
    {
        // Vehicle-specific validation rules
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وسیله نقلیه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);
            
        RuleFor(x => x.VehicleId)
            .NotEmpty()
            .WithMessage(" وسیله نقلیه الزامی است");
            
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("اداره الزامی است");
        
        RuleFor(x => x.PersonnelId)
            .NotEmpty()
            .WithMessage("راننده الزامی است");
    }
}
