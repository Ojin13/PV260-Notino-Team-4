using MediatR;
using Popocatepetl.Application.Commands.Reports;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Dtos;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.Reports;

/// <summary>Stores the latest ARK report for an admin user.</summary>
public sealed class DownloadLatestArkReportCommandHandler(
    IArkReportClient arkReportClient,
    IReportRepository reportRepository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<DownloadLatestArkReportCommand, Result<DownloadLatestArkReportResponse>>
{
    public async Task<Result<DownloadLatestArkReportResponse>> Handle(
        DownloadLatestArkReportCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUserContext.Role != Popocatepetl.Domain.Enums.UserRole.Admin)
        {
            return Result<DownloadLatestArkReportResponse>.Failure("Only admins can download reports.");
        }

        try
        {
            var downloadedReportContent = await arkReportClient.DownloadLatestAsync(cancellationToken);
            var previousLatest = await reportRepository.GetLatestAsync();
            var fileName = $"arkk_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            var report = Report.Create(
                fileName,
                currentUserContext.Email,
                downloadedReportContent,
                isLatest: false);

            await reportRepository.SaveAsync(report);
            await reportRepository.SetLatestAsync(report.Id);

            return Result<DownloadLatestArkReportResponse>.Success(new DownloadLatestArkReportResponse(
                report.Id,
                report.FileName,
                report.UploadedAt,
                previousLatest is not null));
        }
        catch (Exception ex)
        {
            return Result<DownloadLatestArkReportResponse>.Failure(
                $"Failed to download and store the latest report: {ex.Message}");
        }
    }
}
