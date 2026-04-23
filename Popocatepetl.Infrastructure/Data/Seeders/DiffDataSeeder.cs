using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Infrastructure.Data.Seeders;

public static class DiffDataSeeder
{
    private record SeedDiffData(
        Guid Id,
        string Name,
        string Ticker,
        int Shares,
        double SharesDiffPercent,
        ShareDiffType ShareDiffType,
        double WeightPercent);

    private static readonly SeedDiffData[] Entries =
    [
        new(SeedIds.DiffDataAaplId, "Apple Inc", "AAPL", 20, 20.0, ShareDiffType.Increased, 11.10),
        new(SeedIds.DiffDataMsftId, "Microsoft Corp", "MSFT", -5, -6.25, ShareDiffType.Decreased, 8.70),
        new(SeedIds.DiffDataNvdaId, "NVIDIA Corp", "NVDA", 30, 100.0, ShareDiffType.New, 4.20)
    ];

    internal static async Task SeedAsync(PopocatepetlDbContext dbContext)
    {
        foreach (var seed in Entries)
        {
            var exists = await dbContext.DiffData.AnyAsync(x => x.Id == seed.Id);
            if (exists)
                continue;

            var entry = DiffData.Create(
                seed.Name,
                seed.Ticker,
                seed.Shares,
                seed.SharesDiffPercent,
                seed.ShareDiffType,
                seed.WeightPercent,
                seed.Id);
            
            dbContext.DiffData.Add(entry);
        }

        await dbContext.SaveChangesAsync();
    }
}