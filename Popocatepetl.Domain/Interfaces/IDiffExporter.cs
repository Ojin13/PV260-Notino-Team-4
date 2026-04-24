using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Serializes a diff into a portable file format (CSV, PDF).</summary>
public interface IDiffExporter
{
    /// <summary>Renders the diff as a CSV file.</summary>
    byte[] ToCsv(DiffResult diff, IEnumerable<DiffData> rows);

    /// <summary>Renders the diff as a PDF file.</summary>
    byte[] ToPdf(DiffResult diff, IEnumerable<DiffData> rows);
}