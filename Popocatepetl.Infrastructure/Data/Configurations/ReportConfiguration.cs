using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public sealed class ReportConfiguration : BaseEntityConfiguration<Report>
{
    public override void Configure(EntityTypeBuilder<Report> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        builder.Property(x => x.UploadedByEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.RawContent)
            .IsRequired();

        builder.Property(x => x.IsLatest)
            .IsRequired();
    }
}
