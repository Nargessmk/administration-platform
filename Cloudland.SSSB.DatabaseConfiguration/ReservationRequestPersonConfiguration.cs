using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ReservationRequestPersonConfiguration : BaseTableConfiguration<ReservationRequestPerson, Guid>
{
	public override void Configure(EntityTypeBuilder<ReservationRequestPerson> builder)
	{
		base.Configure(builder);
		builder.ToTable("tblReservationRequestPerson");

		builder.HasOne(x => x.Personnel)
			.WithMany(p => p.ReservationRequestPersons)
			.HasForeignKey(x => x.PersonnelId)
			.OnDelete(DeleteBehavior.NoAction)
			.IsRequired();

		builder.HasOne(x => x.ReservationRequestHeader)
			.WithMany(r => r.ReservationRequestPersons)
			.HasForeignKey(x => x.ReservationRequestHeaderId)
			.OnDelete(DeleteBehavior.NoAction)
			.IsRequired();

		builder.HasIndex(x => x.PersonnelId);
		builder.HasIndex(x => x.ReservationRequestHeaderId);
	}
}


