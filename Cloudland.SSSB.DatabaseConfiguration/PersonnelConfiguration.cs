using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class PersonnelConfiguration : BaseTableConfiguration<Personnel, Guid>
{
    public override void Configure(EntityTypeBuilder<Personnel> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblPersonnel");
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Personnel>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}