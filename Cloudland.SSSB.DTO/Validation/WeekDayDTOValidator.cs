using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class WeekDayDTOValidator : BaseDTOValidator<WeekDayDTO, Guid>
{
    public WeekDayDTOValidator()
    {
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("زمان شروع الزامی است");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("زمان پایان الزامی است");
    }
}


