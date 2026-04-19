using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Persistence contract for Report entities.</summary>
public interface IReportRepository
{
    /// <summary>Returns the report currently marked as latest, or null if none exists.</summary>
    Task<Report?> GetLatestAsync();

    /// <summary>Returns the report with the given id, or null if not found.</summary>
    Task<Report?> GetByIdAsync(Guid id);

    /// <summary>Returns all stored reports.</summary>
    Task<IReadOnlyList<Report>> GetAllAsync();

    /// <summary>Persists a new or updated report.</summary>
    Task SaveAsync(Report report);

    /// <summary>
    /// Marks the report with reportId as the latest,
    /// clearing the flag on any previously marked report.
    /// </summary>
    Task SetLatestAsync(Guid reportId);
}
