using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class LocationViewConfiguration : BaseViewConfiguration<LocationView>
{
    public override void Configure(EntityTypeBuilder<LocationView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwLocation");
    }
}
