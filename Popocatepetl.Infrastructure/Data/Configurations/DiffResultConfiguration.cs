using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public sealed class DiffResultConfiguration : BaseEntityConfiguration<DiffResult>
{
    public override void Configure(EntityTypeBuilder<DiffResult> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.BaselineReportId)
            .IsRequired();

        builder.Property(x => x.CurrentReportId)
            .IsRequired();

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.HasOne(x => x.BaselineReport)
            .WithMany()
            .HasForeignKey(x => x.BaselineReportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CurrentReport)
            .WithMany()
            .HasForeignKey(x => x.CurrentReportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DiffDataEntries)
            .WithOne()
            .HasForeignKey("DiffResultId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
