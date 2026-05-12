using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Entities;

public class DiffResult : BaseEntity
{
    public Guid BaselineReportId { get; init; }
    public Guid CurrentReportId { get; init; }
    public Report BaselineReport { get; init; } = null!;
    public Report CurrentReport { get; init; } = null!;
    public ICollection<DiffData> DiffDataEntries { get; init; } = new List<DiffData>();
    public DateTime GeneratedAt { get; init; }

    private DiffResult() { }

    public static DiffResult Create(Guid baselineReportId, Guid currentReportId, DateTime? generatedAt = null, IEnumerable<DiffData>? diffDataEntries = null) =>
        new DiffResult
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            BaselineReportId = baselineReportId,
            CurrentReportId = currentReportId,
            GeneratedAt = generatedAt ?? DateTime.UtcNow,
            DiffDataEntries = diffDataEntries is null ? new List<DiffData>() : new List<DiffData>(diffDataEntries)
        };

    public static DiffResult CreateFromHoldings(
        Guid baselineReportId,
        Guid currentReportId,
        IReadOnlyDictionary<string, HoldingSnapshot> baseline,
        IReadOnlyDictionary<string, HoldingSnapshot> current)
    {
        var diffs = new List<DiffData>();

        foreach (var (key, currentHolding) in current)
        {
            if (!baseline.TryGetValue(key, out var baselineHolding))
            {
                diffs.Add(DiffData.Create(currentHolding.Name, currentHolding.Ticker, currentHolding.Shares, 100, ShareDiffType.New, currentHolding.WeightPercent));
                continue;
            }

            var sharesDelta = currentHolding.Shares - baselineHolding.Shares;
            if (sharesDelta == 0)
                continue;

            var diffPercent = baselineHolding.Shares == 0
                ? 100
                : (double)sharesDelta / baselineHolding.Shares * 100;

            diffs.Add(DiffData.Create(
                currentHolding.Name,
                currentHolding.Ticker,
                currentHolding.Shares,
                diffPercent,
                sharesDelta > 0 ? ShareDiffType.Increased : ShareDiffType.Decreased,
                currentHolding.WeightPercent));
        }

        foreach (var (key, baselineHolding) in baseline)
        {
            if (current.ContainsKey(key))
                continue;

            diffs.Add(DiffData.Create(baselineHolding.Name, baselineHolding.Ticker, -baselineHolding.Shares, -100, ShareDiffType.Decreased, 0));
        }

        return Create(baselineReportId, currentReportId, diffDataEntries: diffs);
    }
}
