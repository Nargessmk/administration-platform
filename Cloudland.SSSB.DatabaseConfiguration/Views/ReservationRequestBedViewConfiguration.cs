using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ReservationRequestBedViewConfiguration : BaseViewConfiguration<ReservationRequestBedView>
{
    public override void Configure(EntityTypeBuilder<ReservationRequestBedView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwReservationRequestBed");
    }
}



