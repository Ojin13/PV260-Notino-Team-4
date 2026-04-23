using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Infrastructure.Repositories;

/// <summary>
/// Temporary in-memory IDiffRepository returning a hardcoded ARKK diff. Exists so the exporter + email
/// flow can be exercised end-to-end before the real diff-computation pipeline lands. Replace with a
/// DB-backed implementation once teammates deliver it.
/// </summary>
public sealed class StubDiffRepository : IDiffRepository
{
    private static readonly DiffResult DummyDiff = new()
    {
        BaselineReportId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        CurrentReportId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        GeneratedAt = new DateTime(2026, 04, 23, 10, 0, 0, DateTimeKind.Utc),
    };

    private static readonly IReadOnlyList<DiffData> DummyRows =
    [
        new() { Id = Guid.NewGuid(), Ticker = "TSLA", Name = "TESLA INC",
                Shares = 3_825_412, SharesDiffPercent = 2.15,
                ShareDiffType = ShareDiffType.Increased, WeightPercent = 9.87 },
        new() { Id = Guid.NewGuid(), Ticker = "ROKU", Name = "ROKU INC",
                Shares = 4_192_005, SharesDiffPercent = -1.34,
                ShareDiffType = ShareDiffType.Decreased, WeightPercent = 7.12 },
        new() { Id = Guid.NewGuid(), Ticker = "COIN", Name = "COINBASE GLOBAL INC -CLASS A",
                Shares = 2_730_811, SharesDiffPercent = 0.82,
                ShareDiffType = ShareDiffType.Increased, WeightPercent = 6.44 },
        new() { Id = Guid.NewGuid(), Ticker = "PATH", Name = "UIPATH INC - CLASS A",
                Shares = 15_204_330, SharesDiffPercent = 5.01,
                ShareDiffType = ShareDiffType.New, WeightPercent = 4.98 },
        new() { Id = Guid.NewGuid(), Ticker = "HOOD", Name = "ROBINHOOD MARKETS INC - CLASS A",
                Shares = 6_120_775, SharesDiffPercent = -3.27,
                ShareDiffType = ShareDiffType.Decreased, WeightPercent = 4.31 },
    ];

    public Task<LatestDiff?> GetLatestAsync(CancellationToken cancellationToken)
        => Task.FromResult<LatestDiff?>(new LatestDiff(DummyDiff, DummyRows));
}