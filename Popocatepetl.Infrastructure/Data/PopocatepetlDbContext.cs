using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Infrastructure.Data.Configurations;

namespace Popocatepetl.Infrastructure.Data;

/// <summary>EF Core database context for the Popocatepetl application.</summary>
public sealed class PopocatepetlDbContext(DbContextOptions<PopocatepetlDbContext> options) : DbContext(options)
{
    /// <summary>All registered users.</summary>
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppUserConfiguration());
    }
}
