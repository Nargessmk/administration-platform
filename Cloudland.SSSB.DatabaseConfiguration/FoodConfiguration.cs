using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class FoodConfiguration : BaseTableConfiguration<Food, Guid>
{
    public override void Configure(EntityTypeBuilder<Food> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblFood");
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Food>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}