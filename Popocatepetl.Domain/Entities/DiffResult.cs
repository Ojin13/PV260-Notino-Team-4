namespace Popocatepetl.Domain.Entities;

/// <summary>Immutable value object representing the diff between two reports.</summary>
public class DiffResult()
{
    public Guid BaselineReportId { get; init; }
    public Guid CurrentReportId { get; init; }
    public IEnumerable<DiffResult> DiffResults { get; init; } = [];
    public DateTime GeneratedAt { get; init; }
}
