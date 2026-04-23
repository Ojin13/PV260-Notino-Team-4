using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Latest generated diff with its associated rows.</summary>
public sealed record LatestDiff(DiffResult Diff, IReadOnlyList<DiffData> Rows);

/// <summary>Read access to the diff aggregate. Writes are owned by the diff-computation pipeline.</summary>
public interface IDiffRepository
{
    /// <summary>Returns the most recently generated diff, or null if none has been generated yet.</summary>
    Task<LatestDiff?> GetLatestAsync(CancellationToken cancellationToken);
}