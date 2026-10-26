using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class AccountNumberDTOValidator : BaseDTOValidator<AccountNumberDTO, Guid>
{
    public AccountNumberDTOValidator()
    {
        RuleFor(x => x.OrganizationalChartId)
            .NotEmpty()
            .WithMessage("شناسه واحد سازمانی الزامی است")
            .NotEqual(Guid.Empty)
            .WithMessage("شناسه واحد سازمانی باید یک GUID معتبر باشد");

        RuleFor(x => x.AccountNumberValue)
            .NotEmpty().WithMessage("شماره حساب الزامی است")
            .MaximumLength(128).WithMessage("شماره حساب نمی‌تواند بیش از 128 کاراکتر باشد");
    }
}



