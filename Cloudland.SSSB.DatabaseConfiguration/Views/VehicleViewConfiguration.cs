using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class VehicleViewConfiguration : BaseViewConfiguration<VehicleView>
{
    public override void Configure(EntityTypeBuilder<VehicleView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwVehicle");
    }
}
