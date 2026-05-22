using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Reports;

/// <summary>Downloads and stores the latest ARK report.</summary>
public sealed record DownloadLatestArkReportCommand : IAuditableRequest,
    IRequest<Result<DownloadLatestArkReportResponse>>
{
    public string ActionName => "DownloadLatestReport";
    public string Detail => "";
}
