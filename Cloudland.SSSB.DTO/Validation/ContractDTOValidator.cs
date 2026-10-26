using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ContractDTOValidator : BaseDTOValidator<ContractDTO, Guid>
{
    public ContractDTOValidator()
    {
         // Rule for Id
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه قرارداد باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);

        // Rule for Contractor
        RuleFor(x => x.ContractorId)
            .NotEmpty().WithMessage("پیمانکار باید انتخاب شود")
            .When(x => x.ParentId == null) // Only apply when ParentId is null
            .Must(id => id != Guid.Empty).WithMessage("شناسه پیمانکار باید یک GUID معتبر باشد")
            .When(x => x.ParentId == null); // Apply GUID validation only when ParentId is null

        // Rule for Contract Number - Only required when ParentId is null (root contracts)
        RuleFor(x => x.ContractNumber)
            .NotEmpty().WithMessage("شماره قرارداد باید وارد شود")
            .When(x => x.ParentId == null) // Only apply when ParentId is null
            .MaximumLength(50).WithMessage("شماره قرارداد نمی‌تواند بیش از 50 کاراکتر باشد")
            .When(x => x.ParentId == null); // Apply length validation only when ParentId is null

        // Rule for Contract Type
        RuleFor(x => x.ContractTypeId)
            .NotEmpty().WithMessage("نوع قرارداد باید وارد شود")
            .Must(id => id != Guid.Empty).WithMessage("شناسه نوع قرارداد باید یک GUID معتبر باشد");

        // Rules for Dates
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("تاریخ شروع باید قبل از تاریخ پایان باشد");

        // Rules for Quantities
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد باید بزرگتر از صفر باشد")
            .When(x => x.ParentId == null); // Only validate quantity for root contracts

        // Rules for Prices
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("قیمت باید بزرگتر از صفر باشد")
            .When(x => x.ParentId == null);

        RuleFor(x => x.AdjustmentPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("درصد تعدیل نمی‌تواند منفی باشد");
    }
}