using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractFoodConfiguration : BaseTableConfiguration<ContractFood, Guid>
{
    public override void Configure(EntityTypeBuilder<ContractFood> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblContractFood");
        
        builder.HasOne(cf => cf.Contract)
            .WithMany(c => c.ContractFoods)
            .HasForeignKey(cf => cf.ContractId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
        
        builder.HasOne(cf => cf.Food)
            .WithMany(f => f.ContractFoods) 
            .HasForeignKey(cf => cf.FoodId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}



