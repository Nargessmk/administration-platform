using FluentValidation;
using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO.Validation;

public abstract class BaseDTOValidator<T, TKey> : AbstractValidator<T> where T : BaseDTO<TKey>
{
    protected BaseDTOValidator()
    {
        // Common validation rules for BaseDTO properties

        RuleFor(x => x.title)
            .MaximumLength(200)
            .WithMessage("عنوان نمی‌تواند بیشتر از 200 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.title));

        RuleFor(x => x.title)
            .MinimumLength(2)
            .WithMessage("عنوان باید حداقل 2 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.title));

        RuleFor(x => x.desc)
            .MaximumLength(1000)
            .WithMessage("توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.desc));

        RuleFor(x => x.UserCreated)
            .NotEqual(Guid.Empty)
            .WithMessage("شناسه کاربر ایجاد کننده الزامی است")
            .When(x => x.UserCreated != default);

        RuleFor(x => x.LastModifiedUser)
            .NotEqual(Guid.Empty)
            .WithMessage("شناسه آخرین کاربر تغییر دهنده الزامی است")
            .When(x => x.LastModifiedUser != default);

        RuleFor(x => x.LastModifiedAt)
            .GreaterThanOrEqualTo(x => x.CreateAt)
            .WithMessage("تاریخ آخرین تغییر باید بعد از تاریخ ایجاد باشد")
            .When(x => x.CreateAt != default && x.LastModifiedAt != default);
    }
}
