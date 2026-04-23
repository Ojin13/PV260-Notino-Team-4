using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Infrastructure.Data.Seeders;

public static class DiffResultSeeder
{
    private record SeedDiffResult(
        Guid BaselineReportId,
        Guid CurrentReportId,
        DateTime GeneratedAt);

    private static readonly SeedDiffResult[] Results =
    [
        new(
            SeedIds.ReportBaselineId,
            SeedIds.ReportCurrentId,
            new DateTime(2026, 02, 01, 8, 0, 0))
    ];

    internal static async Task SeedAsync(PopocatepetlDbContext dbContext)
    {
        foreach (var seed in Results)
        {
            var exists = await dbContext.DiffResults.AnyAsync(x =>
                x.BaselineReportId == seed.BaselineReportId &&
                x.CurrentReportId == seed.CurrentReportId);
            if (exists)
                continue;

            var result = DiffResult.Create(
                seed.BaselineReportId,
                seed.CurrentReportId,
                seed.GeneratedAt,
                [
                    DiffData.Create("Apple Inc", "AAPL", 20, 20.0, ShareDiffType.Increased, 11.10, SeedIds.DiffDataAaplId),
                    DiffData.Create("Microsoft Corp", "MSFT", -5, -6.25, ShareDiffType.Decreased, 8.70, SeedIds.DiffDataMsftId),
                    DiffData.Create("NVIDIA Corp", "NVDA", 30, 100.0, ShareDiffType.New, 4.20, SeedIds.DiffDataNvdaId)
                ]);

            dbContext.DiffResults.Add(result);
        }

        await dbContext.SaveChangesAsync();
    }
}