using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class WeekDayViewConfiguration : BaseViewConfiguration<WeekDayView>
{
    public override void Configure(EntityTypeBuilder<WeekDayView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwWeekDay");
    }
}


