using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ReservationRequestLetterConfiguration : BaseTableConfiguration<ReservationRequestLetter, Guid>
{
    public override void Configure(EntityTypeBuilder<ReservationRequestLetter> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblReservationRequestLetter");

        builder.Property(x => x.LetterDate).IsRequired();
        builder.Property(x => x.LetterNumber).IsRequired().HasMaxLength(128);
        builder.Property(x => x.LetterSubject).IsRequired().HasMaxLength(512);
        builder.Property(x => x.LetterFileUrl).HasMaxLength(1024);

        builder.HasOne(x => x.ReservationRequestHeader)
            .WithMany(r => r.ReservationRequestLetters)
            .HasForeignKey(x => x.ReservationRequestHeaderId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => x.ReservationRequestHeaderId);
        builder.HasIndex(x => x.LetterNumber);
    }
}


