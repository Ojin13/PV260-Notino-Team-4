using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Infrastructure.Data.Seeders;

public static class ReportSeeder
{
    private record SeedReport(
        Guid Id,
        string FileName,
        DateTime UploadedAt,
        string UploadedByEmail,
        string RawContent,
        bool IsLatest);

    private static readonly SeedReport[] Reports =
    [
        new(
            SeedIds.ReportBaselineId,
            "portfolio_baseline_2025-12.csv",
            new DateTime(2025, 12, 31, 18, 30, 0),
            "analyst@popocatepetl.app",
            "Ticker,Name,Shares,WeightPercent\nAAPL,Apple Inc,100,10.50\nMSFT,Microsoft Corp,80,9.20\n",
            false),
        new(
            SeedIds.ReportCurrentId,
            "portfolio_current_2026-01.csv",
            new DateTime(2026, 01, 31, 18, 30, 0),
            "analyst@popocatepetl.app",
            "Ticker,Name,Shares,WeightPercent\nAAPL,Apple Inc,120,11.10\nMSFT,Microsoft Corp,75,8.70\nNVDA,NVIDIA Corp,30,4.20\n",
            true)
    ];

    internal static async Task SeedAsync(PopocatepetlDbContext dbContext)
    {
        foreach (var seed in Reports)
        {
            var exists = await dbContext.Reports.AnyAsync(x => x.Id == seed.Id);
            if (exists)
                continue;

            var report = Report.Create(
                seed.FileName,
                seed.UploadedByEmail,
                seed.RawContent,
                seed.IsLatest,
                seed.UploadedAt,
                seed.Id);

            dbContext.Reports.Add(report);
        }

        await dbContext.SaveChangesAsync();
    }
}
