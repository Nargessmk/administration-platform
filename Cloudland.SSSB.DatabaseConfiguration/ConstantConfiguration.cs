using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ConstantConfiguration : BaseTableConfiguration<Constant, Guid>
{
    public override void Configure(EntityTypeBuilder<Constant> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.title).IsUnicode();

        builder
            .HasMany(p => p.Constants)
            .WithOne(p => p.ParentConstant)
            .HasForeignKey(j => j.ParentId);
    }
}