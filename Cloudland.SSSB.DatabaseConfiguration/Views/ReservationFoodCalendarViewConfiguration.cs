using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ReservationFoodCalendarViewConfiguration : BaseViewConfiguration<ReservationFoodCalendarView>
{
    public override void Configure(EntityTypeBuilder<ReservationFoodCalendarView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwReservationFoodCalendar");
    }
}
