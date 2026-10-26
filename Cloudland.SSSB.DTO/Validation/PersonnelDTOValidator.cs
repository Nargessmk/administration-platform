using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;


public class PersonnelDTOValidator : BaseDTOValidator<PersonnelDTO, Guid>
{
    public PersonnelDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه پرسنل باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rules for Required Personal Info
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("نام باید وارد شود")
            .MaximumLength(100).WithMessage("نام نمی‌تواند بیش از 100 کاراکتر باشد");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("نام خانوادگی باید وارد شود")
            .MaximumLength(100).WithMessage("نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی باید وارد شود")
            .Length(10).WithMessage("کد ملی باید 10 رقمی باشد")
            .Matches("^[0-9]*$").WithMessage("کد ملی باید فقط شامل اعداد باشد");

        // Rules for Contact Info
        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("شماره تماس نمی‌تواند بیش از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Mobile1)
            .MaximumLength(20).WithMessage("موبایل 1 نمی‌تواند بیش از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile1));

        RuleFor(x => x.Mobile2)
            .MaximumLength(20).WithMessage("موبایل 2 نمی‌تواند بیش از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Mobile2));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("آدرس نمی‌تواند بیش از 500 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.DisplayName)
            .MaximumLength(150).WithMessage("نام نمایشی نمی‌تواند بیش از 150 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        // Rules for Required Foreign Keys
        RuleFor(x => x.ContractTypeId)
            .NotEmpty().WithMessage("نوع قرارداد پرسنل باید انتخاب شود")
            .Must(id => id != Guid.Empty).WithMessage("شناسه نوع قرارداد باید یک GUID معتبر باشد");

        // Rules for Optional Foreign Keys
        RuleFor(x => x.FamilyRelationTypeId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("شناسه نسبت فامیلی باید یک GUID معتبر باشد");

        
        RuleFor(x => x.FusionUserId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("شناسه کاربری باید یک GUID معتبر باشد");

        // Rule for Wallet Balance
        RuleFor(x => x.WalletBalance)
            .GreaterThanOrEqualTo(0).WithMessage("موجودی کیف پول نمی‌تواند منفی باشد");
    }
}