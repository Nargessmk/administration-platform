using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class VehicleConfiguration : BaseTableConfiguration<Vehicle, Guid>
{
    public override void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblVehicle");

        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Vehicle>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.VehicleTypeId);
        builder.HasIndex(x => x.LicensePlate).IsUnique();
    }
}
