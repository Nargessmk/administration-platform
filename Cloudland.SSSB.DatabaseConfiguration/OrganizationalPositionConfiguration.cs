using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class OrganizationalPositionConfiguration : BaseTableConfiguration<OrganizationalPosition, Guid>
{
    public override void Configure(EntityTypeBuilder<OrganizationalPosition> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblOrganizationalPosition");
        
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<OrganizationalPosition>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}