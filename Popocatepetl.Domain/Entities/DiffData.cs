using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Entities;

public class DiffData()
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Ticker { get; init; } = string.Empty;
    public int Shares { get; init; }
    public double SharesDiffPercent { get; init; }
    public ShareDiffType ShareDiffType { get; init; }
    public double WeightPercent { get; init; }
}