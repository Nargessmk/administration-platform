using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractViewConfiguration : Views.BaseViewConfiguration<ContractView>
{
    public override void Configure(EntityTypeBuilder<ContractView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwContract");
    }
}



