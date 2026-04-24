using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Configurations;

internal sealed class AppUserConfiguration : BaseEntityConfiguration<AppUser>
{
    public override void Configure(EntityTypeBuilder<AppUser> builder)
    {
        base.Configure(builder);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(300);
    }
}
