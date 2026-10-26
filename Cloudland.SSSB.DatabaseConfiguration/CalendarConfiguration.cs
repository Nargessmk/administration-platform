using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class CalendarConfiguration : BaseTableConfiguration<Calendar, Guid>
{
    public override void Configure(EntityTypeBuilder<Calendar> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblCalendar");
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Calendar>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(h => h.Date)
            .IsRequired();

        builder.Property(h => h.Occasion)
            .HasMaxLength(200);

        builder.Property(h => h.AdditionalDescription)
            .HasMaxLength(1000);

        builder.Property(h => h.IsHoliday)
            .IsRequired();
    }
}


