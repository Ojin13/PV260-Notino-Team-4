using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;

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
                seed.GeneratedAt);

            dbContext.DiffResults.Add(result);
        }

        await dbContext.SaveChangesAsync();
    }
}