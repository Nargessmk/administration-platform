using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ContractFoodDTOValidator : BaseDTOValidator<ContractFoodDTO, Guid>
{
    public ContractFoodDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه قرارداد غذا باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rules for Required Foreign Keys
        RuleFor(x => x.ContractId)
            .NotEmpty()
            .WithMessage("قرارداد باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه قرارداد باید یک GUID معتبر باشد");

        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("غذا باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه غذا باید یک GUID معتبر باشد");

        // Rules for Quantities
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("تعداد باید بزرگتر از صفر باشد");
        

        // Rules for Prices
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("قیمت باید بزرگتر از صفر باشد");

    }
}