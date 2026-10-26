using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class AccountNumberConfiguration : BaseTableConfiguration<AccountNumber, Guid>
{
    public override void Configure(EntityTypeBuilder<AccountNumber> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblAccountNumber");
        
        builder.HasOne(cf => cf.OrganizationalChart)
            .WithMany(f => f.AccountNumbers) 
            .HasForeignKey(cf => cf.OrganizationalChartId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(x => x.OrganizationalChartId);
        builder.Property(x => x.AccountNumberValue).HasMaxLength(128).IsRequired();
    }
}



