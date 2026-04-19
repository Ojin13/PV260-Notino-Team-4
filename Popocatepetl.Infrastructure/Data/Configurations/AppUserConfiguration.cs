using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

/// <summary>EF Core entity configuration for AppUser.</summary>
internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
