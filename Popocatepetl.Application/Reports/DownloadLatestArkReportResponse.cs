namespace Popocatepetl.Application.Reports;

/// <summary>Response returned after downloading and storing the latest ARK report.</summary>
public sealed record DownloadLatestArkReportResponse(
    Guid ReportId,
    string FileName,
    DateTime UploadedAt,
    bool PreviousReportFound);
