using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ReservationRequestLetterViewConfiguration : BaseViewConfiguration<ReservationRequestLetterView>
{
    public override void Configure(EntityTypeBuilder<ReservationRequestLetterView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwReservationRequestLetter");
    }
}


