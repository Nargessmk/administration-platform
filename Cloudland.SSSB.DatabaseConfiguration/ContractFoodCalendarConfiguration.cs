using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractFoodCalendarConfiguration : BaseTableConfiguration<ContractFoodCalendar, Guid>
{
    public override void Configure(EntityTypeBuilder<ContractFoodCalendar> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblContractFoodCalendar");
        
        builder.HasOne(x => x.ContractFoodLocation)
            .WithMany(cf => cf.ContractFoodCalendars)
            .HasForeignKey(x => x.ContractFoodLocationId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.Meal)
            .WithMany(cf => cf.ContractFoodCalendars)
            .HasForeignKey(x => x.MealId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasIndex(x => new { x.Date }).IsUnique(false);
    }
}


