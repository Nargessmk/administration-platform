using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ReservationRequestHeaderConfiguration : BaseTableConfiguration<ReservationRequestHeader, Guid>
{
public override void Configure(EntityTypeBuilder<ReservationRequestHeader> builder)
{
    base.Configure(builder);
    builder.ToTable("tblReservationRequestHeader");

    // Set default values for ALL nullable Guid fields
    builder.Property(x => x.ForDepartmentId)
        .IsRequired(false);

    builder.Property(x => x.FromDepartmentId)
        .IsRequired(false);

    builder.Property(x => x.RequestingOrganizationalUnitId)
        .IsRequired(false);


    builder.Property(x => x.VehicleId)
        .IsRequired(false);


    builder.Property(x => x.DriverId)
        .IsRequired(false)
        .HasDefaultValue(Guid.Empty);

    builder.Property(x => x.LocationId)
        .IsRequired(false);

    builder.Property(x => x.RequestTypeId)
        .IsRequired(false);

    builder.Property(x => x.ShiftChangeDayId)
        .IsRequired(false);

    // Original property configurations
    builder.Property(x => x.FromDate).IsRequired(false);
    builder.Property(x => x.ToDate).IsRequired(false);

    // Relationship configurations (unchanged)
    builder.HasOne(x => x.AccountNumber)
        .WithMany()
        .HasForeignKey(x => x.AccountNumberId)
        .OnDelete(DeleteBehavior.NoAction)
        .IsRequired();

    builder.HasOne(x => x.FromDepartment)
        .WithMany()
        .HasForeignKey(x => x.FromDepartmentId)
        .OnDelete(DeleteBehavior.NoAction)
        .IsRequired(false);

    builder.HasOne(x => x.RequestingOrganizationalUnit)
        .WithMany()
        .HasForeignKey(x => x.RequestingOrganizationalUnitId)
        .OnDelete(DeleteBehavior.NoAction)
        .IsRequired(false);
    
    builder.HasOne(x => x.Location)
        .WithMany()
        .HasForeignKey(x => x.LocationId)
        .OnDelete(DeleteBehavior.NoAction)
        .IsRequired(false);

    // Index configurations (unchanged)
    builder.HasIndex(x => x.AccountNumberId);
    builder.HasIndex(x => x.FromDepartmentId);
    builder.HasIndex(x => x.ForDepartmentId);
    builder.HasIndex(x => x.RequestingOrganizationalUnitId);
    builder.HasIndex(x => x.VehicleId);
    builder.HasIndex(x => x.DriverId);
    builder.HasIndex(x => x.LocationId);
}
}


