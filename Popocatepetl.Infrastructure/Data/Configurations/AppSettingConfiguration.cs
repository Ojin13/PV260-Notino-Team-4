using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Infrastructure.Data.Configurations;

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(x => x.Type);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(256);

        builder.Property(x => x.Value)
            .IsRequired();
    }
}
