using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Dtos;

namespace Popocatepetl.Application.Commands.Reports;

/// <summary>Downloads and stores the latest ARK report.</summary>
public sealed record DownloadLatestArkReportCommand : IRequest<Result<DownloadLatestArkReportResponse>>;
