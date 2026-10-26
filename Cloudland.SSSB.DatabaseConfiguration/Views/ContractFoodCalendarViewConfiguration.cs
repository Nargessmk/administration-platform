using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ContractFoodCalendarViewConfiguration : BaseViewConfiguration<ContractFoodCalendarView>
{
    public override void Configure(EntityTypeBuilder<ContractFoodCalendarView> builder)
    {
        base.Configure(builder);
        builder.ToView("viwContractFoodCalendar");
    }
}
