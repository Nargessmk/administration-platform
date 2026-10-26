using FluentValidation;

namespace Cloudland.SSSB.DTO.Validation;

public class ReservationRequestHeaderDTOValidator : BaseDTOValidator<ReservationRequestHeaderDTO, Guid>
{
    public ReservationRequestHeaderDTOValidator()
    {
        RuleFor(x => x.Id)
            .Must(id => id != Guid.Empty)
            .WithMessage("شناسه باید یک GUID معتبر باشد")
            .When(x => x.Id != Guid.Empty);
        
        RuleFor(x => x.AccountNumberId)
            .NotEmpty().WithMessage("شماره حساب باید انتخاب شود")
            .Must(id => id == null || id != Guid.Empty).WithMessage("شناسه شماره حساب باید معتبر باشد");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("مکان باید انتخاب شود")
            .Must(id => id == null || id != Guid.Empty).WithMessage("شناسه مکان باید معتبر باشد");
        
    }
}


