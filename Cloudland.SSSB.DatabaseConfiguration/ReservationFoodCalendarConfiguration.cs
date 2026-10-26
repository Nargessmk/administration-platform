using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ReservationFoodCalendarConfiguration : BaseTableConfiguration<ReservationFoodCalendar, Guid>
{
    public override void Configure(EntityTypeBuilder<ReservationFoodCalendar> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblReservationFoodCalendar");

        builder.Property(x => x.ReservationDate).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.CancelationStatus).IsRequired();

        builder.HasOne(x => x.ReservationRequestHeader)
            .WithMany(r => r.FoodReservations)
            .HasForeignKey(x => x.ReservationRequestHeaderId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();


        builder.HasOne(x => x.ContractFoodCalendar)
            .WithMany()
            .HasForeignKey(x => x.ContractFoodCalendarId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => x.ReservationRequestHeaderId);
        builder.HasIndex(x => x.PersonnelId);
        builder.HasIndex(x => x.ContractFoodCalendarId);
        builder.HasIndex(x => x.ReservationDate);
    }
}


