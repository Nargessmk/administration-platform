using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class OrganizationalChartDTOValidator : BaseDTOValidator<OrganizationalChartDTO, Guid>
{
    public OrganizationalChartDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه سازمان باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        RuleFor(x => x.Address)
            .MaximumLength(512)
            .WithMessage("آدرس نمی‌تواند بیش از 512 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(64)
            .WithMessage("شماره تماس نمی‌تواند بیش از 64 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Body)
            .MaximumLength(4000)
            .WithMessage("متن نمی‌تواند بیش از 4000 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.Body));

        RuleFor(x => x.Head)
            .Must(g => g == null || g != Guid.Empty)
            .WithMessage("شناسه مدیر باید GUID معتبر باشد");

        RuleFor(x => x.Alternative)
            .Must(g => g == null || g != Guid.Empty)
            .WithMessage("شناسه جانشین باید GUID معتبر باشد");

    }
    
}
