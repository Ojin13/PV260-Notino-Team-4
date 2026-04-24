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

    private record SeedDiffData(
        Guid Id,
        string Name,
        string Ticker,
        int Shares,
        double SharesDiffPercent,
        ShareDiffType ShareDiffType,
        double WeightPercent);

    private static readonly SeedDiffResult[] Results =
    [
        new(
            SeedIds.ReportBaselineId,
            SeedIds.ReportCurrentId,
            new DateTime(2026, 02, 01, 8, 0, 0))
    ];

    private static readonly SeedDiffData[] Entries =
    [
        new(SeedIds.DiffDataAaplId, "Apple Inc", "AAPL", 20, 20.0, ShareDiffType.Increased, 11.10),
        new(SeedIds.DiffDataMsftId, "Microsoft Corp", "MSFT", -5, -6.25, ShareDiffType.Decreased, 8.70),
        new(SeedIds.DiffDataNvdaId, "NVIDIA Corp", "NVDA", 30, 100.0, ShareDiffType.New, 4.20)
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
                seed.GeneratedAt);

            dbContext.DiffResults.Add(result);

            foreach (var entry in Entries)
            {
                var diffData = dbContext.DiffData.Local.FirstOrDefault(x => x.Id == entry.Id)
                    ?? await dbContext.DiffData.FirstOrDefaultAsync(x => x.Id == entry.Id);

                if (diffData is null)
                {
                    diffData = DiffData.Create(
                        entry.Name,
                        entry.Ticker,
                        entry.Shares,
                        entry.SharesDiffPercent,
                        entry.ShareDiffType,
                        entry.WeightPercent,
                        entry.Id);

                    dbContext.DiffData.Add(diffData);
                }

                // DiffResultId is currently a shadow FK, so set it via the ChangeTracker.
                dbContext.Entry(diffData).Property<Guid?>("DiffResultId").CurrentValue = result.Id;
            }
        }

        await dbContext.SaveChangesAsync();
    }
}