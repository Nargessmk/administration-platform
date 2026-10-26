using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ContractFoodCalendarDTOValidator : BaseDTOValidator<ContractFoodCalendarDTO, Guid>
{
    public ContractFoodCalendarDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .When(x => x.Id != Guid.Empty);

        RuleFor(x => x.MealId)
            .NotEmpty()
            .Must(id => id != Guid.Empty);

        RuleFor(x => x.Date)
            .NotEmpty();

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0);
    }
}


