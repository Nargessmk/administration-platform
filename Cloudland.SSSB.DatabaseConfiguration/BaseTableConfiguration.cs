using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.SSSB.DatabaseConfiguration;

public class BaseTableConfiguration<T, KEY> : IEntityTypeConfiguration<T> where T : BaseModel<KEY>
{
    private readonly string? CustomerID;

    public BaseTableConfiguration()
    {
    }

    public BaseTableConfiguration(string? customerId)
    {
        CustomerID = customerId;
    }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        // builder.HasQueryFilter(x => CustomerID != null && x.CustomerId == new Guid(CustomerID));
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}