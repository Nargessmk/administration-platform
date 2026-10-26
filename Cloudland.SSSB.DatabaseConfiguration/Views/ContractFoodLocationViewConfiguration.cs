using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ContractFoodLocationViewConfiguration : BaseViewConfiguration<ContractFoodLocationView>
{
    public override void Configure(EntityTypeBuilder<ContractFoodLocationView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwContractFoodLocation");
    }
}


