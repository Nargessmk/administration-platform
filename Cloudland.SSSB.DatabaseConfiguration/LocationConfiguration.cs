using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class LocationConfiguration : BaseTableConfiguration<Location, Guid>
{
    public override void Configure(EntityTypeBuilder<Location> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblLocation");
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Location>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}