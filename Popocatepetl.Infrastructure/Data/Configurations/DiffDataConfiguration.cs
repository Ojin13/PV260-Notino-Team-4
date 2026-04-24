using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public sealed class DiffDataConfiguration : BaseEntityConfiguration<DiffData>
{
    public override void Configure(EntityTypeBuilder<DiffData> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Ticker)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Shares)
            .IsRequired();

        builder.Property(x => x.SharesDiffPercent)
            .IsRequired();

        builder.Property(x => x.ShareDiffType)
            .IsRequired();

        builder.Property(x => x.WeightPercent)
            .IsRequired();
    }
}
