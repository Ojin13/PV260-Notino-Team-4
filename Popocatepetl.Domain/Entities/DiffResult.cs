namespace Popocatepetl.Domain.Entities;

/// <summary>Immutable value object representing the diff between two reports.</summary>
public class DiffResult()
{
    public Guid BaselineReportId { get; init; }
    public Guid CurrentReportId { get; init; }
    public Report BaselineReport { get; init; } = null!;
    public Report CurrentReport { get; init; } = null!;
    public IEnumerable<DiffData> DiffDataEntries { get; init; } = [];
    public DateTime GeneratedAt { get; init; }
}
