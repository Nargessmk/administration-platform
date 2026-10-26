using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ContractorViewConfiguration : BaseViewConfiguration<ContractorView>
{
    public override void Configure(EntityTypeBuilder<ContractorView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwContractor");
    }
}
