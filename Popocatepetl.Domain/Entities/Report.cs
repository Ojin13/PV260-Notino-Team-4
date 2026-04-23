namespace Popocatepetl.Domain.Entities;

public class Report : BaseEntity
{
    public string FileName { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public string UploadedByEmail { get; init; } = string.Empty;
    public string RawContent { get; init; } = string.Empty;
    public bool IsLatest { get; set; }

    private Report() { }

    public static Report Create(string fileName, string uploadedByEmail, string rawContent, bool isLatest, DateTime? uploadedAt = null, Guid? id = null) =>
        new Report
        {
            Id = id ?? Guid.NewGuid(),
            CreatedAt = uploadedAt ?? DateTime.UtcNow,
            FileName = fileName,
            UploadedAt = uploadedAt ?? DateTime.UtcNow,
            UploadedByEmail = uploadedByEmail,
            RawContent = rawContent,
            IsLatest = isLatest
        };
}
