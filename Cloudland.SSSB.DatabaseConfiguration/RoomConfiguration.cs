using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class RoomConfiguration : BaseTableConfiguration<Room, Guid>
{
    public override void Configure(EntityTypeBuilder<Room> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblRoom");
        
        builder.HasOne(cf => cf.Location)
            .WithMany(f => f.Rooms) 
            .HasForeignKey(cf => cf.LocationId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }
}