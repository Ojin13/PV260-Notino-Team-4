using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public sealed class AuditLogConfiguration : BaseEntityConfiguration<AuditLog>
{
    public override void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.UserEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.WasSuccessful)
            .IsRequired();

        builder.Property(x => x.Details)
            .HasMaxLength(4000);
    }
}
