using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;


public class OrganizationalPositionDTOValidator : BaseDTOValidator<OrganizationalPositionDTO, Guid>
{
    public OrganizationalPositionDTOValidator()
    {
        // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه سمت سازمانی باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rule for Address
        RuleFor(x => x.PositionCode)
            .MaximumLength(20)
            .WithMessage("کد سازمانی نمی‌تواند بیش از 20 کاراکتر باشد")
            .When(x => !string.IsNullOrWhiteSpace(x.PositionCode));

    }
}