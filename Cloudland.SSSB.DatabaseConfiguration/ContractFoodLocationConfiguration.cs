using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractFoodLocationConfiguration : BaseTableConfiguration<ContractFoodLocation, Guid>
{
    public override void Configure(EntityTypeBuilder<ContractFoodLocation> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblContractFoodLocation");

        builder.HasOne(x => x.ContractFood)
            .WithMany(cf => cf.ContractFoodLocations)
            .HasForeignKey(x => x.ContractFoodId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(x => x.ContractLocation)
            .WithMany(cl => cl.ContractFoodLocations)
            .HasForeignKey(x => x.ContractLocationId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}


