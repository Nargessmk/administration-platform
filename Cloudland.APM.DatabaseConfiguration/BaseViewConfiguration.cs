using Cloudland.APM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cloudland.APM.DatabaseConfiguration;

public class BaseViewConfiguration<T, KEY> : IEntityTypeConfiguration<T> where T : BaseModel<KEY>
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasNoKey();
    }
}