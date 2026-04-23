using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Entities;

public class DiffData : BaseEntity
{
    public string Name { get; init; } = string.Empty;
    public string Ticker { get; init; } = string.Empty;
    public int Shares { get; init; }
    public double SharesDiffPercent { get; init; }
    public ShareDiffType ShareDiffType { get; init; }
    public double WeightPercent { get; init; }

    private DiffData() { }

    public static DiffData Create(string name, string ticker, int shares, double sharesDiffPercent, ShareDiffType shareDiffType, double weightPercent, Guid? id = null) =>
        new DiffData
        {
            Id = id ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Name = name,
            Ticker = ticker,
            Shares = shares,
            SharesDiffPercent = sharesDiffPercent,
            ShareDiffType = shareDiffType,
            WeightPercent = weightPercent
        };
}
