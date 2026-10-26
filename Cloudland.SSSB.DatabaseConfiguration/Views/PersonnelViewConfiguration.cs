using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class PersonnelViewConfiguration : BaseViewConfiguration<PersonnelView>
{
    public override void Configure(EntityTypeBuilder<PersonnelView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwPersonnel");
    }
}
