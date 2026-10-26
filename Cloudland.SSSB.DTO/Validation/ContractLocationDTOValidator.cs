using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ContractLocationDTOValidator : BaseDTOValidator<ContractLocationDTO, Guid>
{
    public ContractLocationDTOValidator()
    {
         // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه قرارداد باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rule for Contract
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("قرارداد باید انتخاب شود")
            .Must(id => id != Guid.Empty).WithMessage("شناسه قرارداد باید یک GUID معتبر باشد");

        // Rule for Location
        RuleFor(x => x.ContractId)
            .NotEmpty().WithMessage("مکان قرارداد باید انتخاب شود")
            .Must(id => id != Guid.Empty).WithMessage("شناسه مکان قرارداد باید یک GUID معتبر باشد");

    }
}