using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ReservationRequestHeaderViewConfiguration : BaseViewConfiguration<ReservationRequestHeaderView>
{
    public override void Configure(EntityTypeBuilder<ReservationRequestHeaderView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwReservationRequestHeader");
    }
}


