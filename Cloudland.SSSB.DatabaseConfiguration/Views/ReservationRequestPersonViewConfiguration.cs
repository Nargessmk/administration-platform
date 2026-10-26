using Cloudland.SSSB.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration.Views;

public sealed class ReservationRequestPersonViewConfiguration : BaseViewConfiguration<ReservationRequestPersonView>
{
	public override void Configure(EntityTypeBuilder<ReservationRequestPersonView> builder)
	{
		base.Configure(builder);
		builder.ToView("viwReservationRequestPerson");
	}
}


