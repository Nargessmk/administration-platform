using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class VehicleDriverConfiguration : BaseTableConfiguration<VehicleDriver, Guid>
{
    public override void Configure(EntityTypeBuilder<VehicleDriver> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblVehicleDriver");
        
        builder.HasOne(cf => cf.Personnel)
            .WithMany(c => c.VehicleDrivers)
            .HasForeignKey(cf => cf.PersonnelId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
        
        builder.HasOne(cf => cf.OrganizationalChart)
            .WithMany(c => c.VehicleDrivers)
            .HasForeignKey(cf => cf.DepartmentId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
        
        builder.HasOne(cf => cf.Vehicle)
            .WithMany(c => c.VehicleDrivers)
            .HasForeignKey(cf => cf.VehicleId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
        
        builder.HasIndex(x => x.VehicleId);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.PersonnelId);
    }
}
