namespace Popocatepetl.Domain.Entities;

public sealed record HoldingSnapshot(
    string Ticker,
    string Name,
    int Shares,
    double WeightPercent);
