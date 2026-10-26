using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class BedConfiguration : BaseTableConfiguration<Bed, Guid>
{
    public override void Configure(EntityTypeBuilder<Bed> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblBed");
        
        builder.HasOne(b => b.Room)
            .WithMany(r => r.Beds) 
            .HasForeignKey(cf => cf.RoomId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}