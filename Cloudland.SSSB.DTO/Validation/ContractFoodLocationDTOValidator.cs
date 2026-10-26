using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ContractFoodLocationDTOValidator : BaseDTOValidator<ContractFoodLocationDTO, Guid>
{
    public ContractFoodLocationDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        RuleFor(x => x.ContractFoodId)
            .NotEmpty().WithMessage("غذای قرارداد باید انتخاب شود")
            .Must(id => id != Guid.Empty).WithMessage("شناسه غذای قرارداد باید معتبر باشد");

        RuleFor(x => x.ContractLocationId)
            .NotEmpty().WithMessage("مکان قرارداد باید انتخاب شود")
            .Must(id => id != Guid.Empty).WithMessage("شناسه مکان قرارداد باید معتبر باشد");
    }
}


