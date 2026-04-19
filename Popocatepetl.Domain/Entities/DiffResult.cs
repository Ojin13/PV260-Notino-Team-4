namespace Popocatepetl.Domain.Entities;

/// <summary>Immutable value object representing the diff between two reports.</summary>
public record DiffResult(
    Guid BaselineReportId,
    Guid CurrentReportId,
    IReadOnlyList<string> AddedRows,
    IReadOnlyList<string> RemovedRows,
    IReadOnlyList<string> ChangedRows,
    DateTime GeneratedAt);
