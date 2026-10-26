using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class OrganizationalPositionViewConfiguration : BaseViewConfiguration<OrganizationalPositionView>
{
    public override void Configure(EntityTypeBuilder<OrganizationalPositionView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwOrganizationalPosition");
    }
}
