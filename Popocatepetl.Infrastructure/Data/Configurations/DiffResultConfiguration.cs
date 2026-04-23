using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public sealed class DiffResultConfiguration : IEntityTypeConfiguration<DiffResult>
{
    public void Configure(EntityTypeBuilder<DiffResult> builder)
    {
        builder.HasNoKey();

        builder.Property(x => x.BaselineReportId)
            .IsRequired();

        builder.Property(x => x.CurrentReportId)
            .IsRequired();

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.Ignore(x => x.DiffResults);
    }
}