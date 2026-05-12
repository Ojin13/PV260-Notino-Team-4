namespace Popocatepetl.Domain.Interfaces;

/// <summary>Downloads the latest ARK report CSV.</summary>
public interface IArkReportClient
{
    /// <summary>Downloads report content as CSV.</summary>
    Task<string> DownloadLatestAsync(CancellationToken cancellationToken);
}
