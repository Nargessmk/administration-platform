using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;


public class MealDTOValidator : BaseDTOValidator<MealDTO, Guid>
{
    public MealDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه وعده غذایی باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rules for Required TimeSpans
        RuleFor(x => x.StartTime)
            .NotNull()
            .WithMessage("ساعت شروع باید وارد شود");

        RuleFor(x => x.EndTime)
            .NotNull()
            .WithMessage("ساعت پایان باید وارد شود");

        RuleFor(x => x.ReservationEndTime)
            .NotNull()
            .WithMessage("ساعت پایان رزرو باید وارد شود");

        // Rule for optional TimeSpan
        RuleFor(x => x.DefaultReservationTime)
            .NotNull()
            .WithMessage("ساعت پیشفرض رزرو غذا باید مقدار معتبر داشته باشد")
            .When(x => !string.IsNullOrEmpty(x.DefaultReservationTime));
        

        RuleFor(x => x.MealCategoryId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("شناسه دسته‌بندی وعده باید یک GUID معتبر باشد");
    }
}