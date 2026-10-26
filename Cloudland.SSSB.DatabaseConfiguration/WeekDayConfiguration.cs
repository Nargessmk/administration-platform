using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class WeekDayConfiguration : BaseTableConfiguration<WeekDay, Guid>
{
    public override void Configure(EntityTypeBuilder<WeekDay> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblWeekDay");
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<WeekDay>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
    }
}


