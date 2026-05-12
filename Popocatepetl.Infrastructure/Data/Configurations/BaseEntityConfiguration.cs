using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
