using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Seeders;

public static class AuditLogSeeder
{

    private record SeedAuditLog(
        string UserEmail,
        string Action,
        DateTime OccuredAt,
        bool WasSuccessful,
        string? Details);

    private static readonly SeedAuditLog[] Logs =
    [
        new("analyst@popocatepetl.app", "ReportUpload", new DateTime(2025, 12, 31, 18, 30, 10), true, "Uploaded portfolio_baseline_2025-12.csv"),
        new("analyst@popocatepetl.app", "ReportUpload", new DateTime(2026, 01, 31, 18, 30, 15), true, "Uploaded portfolio_current_2026-01.csv"),
        new("analyst@popocatepetl.app", "DiffGeneration", new DateTime(2026, 02, 01, 8, 0, 0), true, "Generated diff for baseline/current monthly reports"),
        new("analyst@popocatepetl.app", "ReportDownload", new DateTime(2026, 02, 01, 8, 15, 22), true, "Downloaded diff summary"),
        new("admin@popocatepetl.app", "AdminAccess", new DateTime(2026, 02, 02, 10, 5, 0), true, "Opened administration dashboard"),
        new("admin@popocatepetl.app", "SettingsUpdate", new DateTime(2026, 02, 02, 10, 7, 41), true, "Changed Diff.ThresholdPercent from 3.0 to 2.5"),
        new("admin@popocatepetl.app", "Login", new DateTime(2026, 02, 03, 8, 2, 18), false, "Two-factor token expired")
    ];

    internal static async Task SeedAsync(PopocatepetlDbContext dbContext)
    {
        foreach (var seed in Logs)
        {
            var exists = await dbContext.AuditLogs.AnyAsync(x =>
                x.UserEmail == seed.UserEmail &&
                x.Action == seed.Action &&
                x.OccurredAt == seed.OccuredAt);
            if (exists)
                continue;
            
            var log = AuditLog.Create(seed.UserEmail, seed.Action, seed.OccuredAt, seed.WasSuccessful, seed.Details);
            dbContext.AuditLogs.Add(log);
        }
        
        await dbContext.SaveChangesAsync();
    }

}