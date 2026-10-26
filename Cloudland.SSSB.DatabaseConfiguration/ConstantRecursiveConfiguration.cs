using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ConstantRecursiveConfiguration : IEntityTypeConfiguration<ConstantRecursive>
{
    public void Configure(EntityTypeBuilder<ConstantRecursive> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);
    }
}