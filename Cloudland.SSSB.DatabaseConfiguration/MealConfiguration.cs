using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class MealConfiguration : BaseTableConfiguration<Meal, Guid>
{
    public override void Configure(EntityTypeBuilder<Meal> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblMeal");
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Meal>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}