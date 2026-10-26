using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractFoodViewConfiguration : Views.BaseViewConfiguration<ContractFoodView>
{
    public override void Configure(EntityTypeBuilder<ContractFoodView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwContractFood");
    }
}



