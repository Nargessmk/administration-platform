using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractLocationViewConfiguration : Views.BaseViewConfiguration<ContractLocationView>
{
    public override void Configure(EntityTypeBuilder<ContractLocationView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwContractLocation");
    }
}



