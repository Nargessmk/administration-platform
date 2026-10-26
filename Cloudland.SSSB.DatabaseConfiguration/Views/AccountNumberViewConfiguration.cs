using Cloudland.SSSB.DatabaseConfiguration.Views;
using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class AccountNumberViewConfiguration : BaseViewConfiguration<AccountNumberView>
{
    public override void Configure(EntityTypeBuilder<AccountNumberView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwAccountNumber");
    }
}
