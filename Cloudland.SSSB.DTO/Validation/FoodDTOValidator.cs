using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;


public class FoodDTOValidator : BaseDTOValidator<FoodDTO, Guid>
{
    public FoodDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه غذا باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rules for Required Foreign Keys
        RuleFor(x => x.FoodCategoryId)
            .NotEmpty()
            .WithMessage("دسته‌بندی غذا باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه دسته‌بندی غذا باید یک GUID معتبر باشد");

        RuleFor(x => x.MealId)
            .NotEmpty()
            .WithMessage("وعده غذایی باید انتخاب شود")
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وعده غذایی باید یک GUID معتبر باشد");
    }
}