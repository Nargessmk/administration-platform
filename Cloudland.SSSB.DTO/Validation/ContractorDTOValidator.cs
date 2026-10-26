using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ContractorDTOValidator : BaseDTOValidator<ContractorDTO, Guid>
{
    public ContractorDTOValidator()
    {
        // Rule for Id (inherited from BaseModel<Guid>)
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه پیمانکار باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rules for string properties based on MaxLength Attribute
        RuleFor(x => x.FirstName)
            .MaximumLength(64)
            .WithMessage("نام نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(64)
            .WithMessage("نام خانوادگی نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Address)
            .MaximumLength(512)
            .WithMessage("آدرس نمی‌تواند بیش از 512 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(64)
            .WithMessage("شماره تماس نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.MobileNumber)
            .MaximumLength(64)
            .WithMessage("شماره موبایل نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.MobileNumber));
        
        RuleFor(x => x.MobileNumber)
            .MaximumLength(64)
            .WithMessage("شماره اقتصادی نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.EconomicNumber));
        
        RuleFor(x => x.MobileNumber)
            .MaximumLength(64)
            .WithMessage("شماره ثبت نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.RegisterNumber));
        
        RuleFor(x => x.MobileNumber)
            .MaximumLength(10)
            .WithMessage("کد ملی نمی‌تواند بیش از 10 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.NationalCode));

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .WithMessage("کد پستی نمی‌تواند بیش از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));

        RuleFor(x => x.AccountNumber)
            .MaximumLength(64)
            .WithMessage("شماره حساب نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.AccountNumber));
        
        RuleFor(x => x.IsActive)
            .NotNull()
            .WithMessage("وضعیت فعال بودن (الزامی است)");
    }
}