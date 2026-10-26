using Cloudland.SSSB.DatabaseConfiguration.Views;
using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class FoodViewConfiguration : BaseViewConfiguration<FoodView>
{
    public override void Configure(EntityTypeBuilder<FoodView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwFood");
    }
}
