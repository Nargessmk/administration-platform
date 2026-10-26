using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class BedViewConfiguration : BaseViewConfiguration<BedView>
{
    public override void Configure(EntityTypeBuilder<BedView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwBed");
    }
}
