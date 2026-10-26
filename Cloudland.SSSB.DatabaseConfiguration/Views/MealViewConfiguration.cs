using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class MealViewConfiguration : BaseViewConfiguration<MealView>
{
    public override void Configure(EntityTypeBuilder<MealView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwMeal");
    }
}
