using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractorConfiguration : BaseTableConfiguration<Contractor, Guid>
{
    public override void Configure(EntityTypeBuilder<Contractor> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblContractor");
        
        builder
            .HasOne(x => x.Constant)
            .WithOne()
            .HasForeignKey<Contractor>(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



