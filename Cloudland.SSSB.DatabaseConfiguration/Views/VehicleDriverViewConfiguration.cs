using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class VehicleDriverViewConfiguration : BaseViewConfiguration<VehicleDriverView>
{
    public override void Configure(EntityTypeBuilder<VehicleDriverView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwVehicleDriver");
    }
}
