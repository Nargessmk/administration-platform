using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractLocationConfiguration : BaseTableConfiguration<ContractLocation, Guid>
{
    public override void Configure(EntityTypeBuilder<ContractLocation> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblContractLocation");

        builder.HasOne(cf => cf.Contract)
            .WithMany(c => c.ContractLocations)
            .HasForeignKey(cf => cf.ContractId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
        
        builder.HasOne(cf => cf.Location)
            .WithMany(c => c.ContractLocations)
            .HasForeignKey(cf => cf.LocationId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}



