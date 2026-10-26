using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class RoomViewConfiguration : BaseViewConfiguration<RoomView>
{
    public override void Configure(EntityTypeBuilder<RoomView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwRoom");
    }
}
