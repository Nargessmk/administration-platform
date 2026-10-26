using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ReservationRequestBedConfiguration : BaseTableConfiguration<ReservationRequestBed, Guid>
{
    public override void Configure(EntityTypeBuilder<ReservationRequestBed> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblReservationRequestBed");

        builder.Property(x => x.ReservationDate).IsRequired();
        builder.Property(x => x.CancellationStatus).IsRequired();

        builder.HasOne(x => x.ReservationRequestHeader)
            .WithMany(r => r.ReservationRequestBeds)
            .HasForeignKey(x => x.ReservationRequestHeaderId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => x.ReservationRequestHeaderId);
        builder.HasIndex(x => x.PersonnelId);
        builder.HasIndex(x => x.ReservationDate);
    }
}


