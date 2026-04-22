namespace Popocatepetl.Domain.Entities;

/// <summary>Represents an uploaded CSV report.</summary>
public class Report
{
    /// <summary>Unique identifier for the report.</summary>
    public Guid Id { get; init; }

    /// <summary>Original file name of the uploaded CSV.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>When the report was uploaded.</summary>
    public DateTime UploadedAt { get; init; }

    /// <summary>Email of the user who uploaded the report.</summary>
    public string UploadedByEmail { get; init; } = string.Empty;

    /// <summary>Full raw CSV text content of the report.</summary>
    public string RawContent { get; init; } = string.Empty;

    /// <summary>Whether this is the most recently uploaded report.</summary>
    public bool IsLatest { get; set; }
}
