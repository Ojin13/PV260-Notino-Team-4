using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Contract for exporting domain objects to PDF.</summary>
public interface IPdfExporter
{
    /// <summary>Renders the given diff as a PDF and returns the raw bytes.</summary>
    Task<byte[]> ExportDiffAsync(DiffResult diff);
}
