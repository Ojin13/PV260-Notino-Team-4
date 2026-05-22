using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Entities;

public class DiffData : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Ticker { get; private set; } = string.Empty;
    public int Shares { get; private set; }
    public double SharesDiffPercent { get; private set; }
    public ShareDiffType ShareDiffType { get; private set; }
    public double WeightPercent { get; private set; }

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
