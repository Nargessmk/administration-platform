using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class OrganizationalChartConfiguration : BaseTableConfiguration<OrganizationalChart, Guid>
{
    public override void Configure(EntityTypeBuilder<OrganizationalChart> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblOrganizationalChart");
        
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<OrganizationalChart>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



