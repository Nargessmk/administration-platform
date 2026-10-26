using Cloudland.SSSB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public sealed class ContractConfiguration : BaseTableConfiguration<Contract, Guid>
{
    public override void Configure(EntityTypeBuilder<Contract> builder)
    {
        base.Configure(builder);
        builder.ToTable("tblContract");

    }
}



