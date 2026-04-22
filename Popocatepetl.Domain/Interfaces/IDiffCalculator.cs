using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Contract for computing the diff between two reports.</summary>
public interface IDiffCalculator
{
    /// <summary>
    /// Compares baseline against current and returns
    /// a DiffResult describing added, removed, and changed rows.
    /// </summary>
    DiffResult Calculate(Report baseline, Report current);
}
