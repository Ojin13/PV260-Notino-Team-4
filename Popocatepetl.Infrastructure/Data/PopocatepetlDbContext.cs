using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data;

/// <summary>EF Core database context for the Popocatepetl application.</summary>
public sealed class PopocatepetlDbContext(DbContextOptions<PopocatepetlDbContext> options) : DbContext(options)
{
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DiffResult> DiffResults => Set<DiffResult>();
    public DbSet<DiffData> DiffData => Set<DiffData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PopocatepetlDbContext).Assembly);
    }
}
