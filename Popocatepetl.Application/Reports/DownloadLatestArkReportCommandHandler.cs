using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Reports;

/// <summary>Stores the latest ARK report for an admin user and recalculates the diff if a baseline exists.</summary>
public sealed class DownloadLatestArkReportCommandHandler(
    IArkReportClient arkReportClient,
    IReportRepository reportRepository,
    IDiffResultRepository diffResultRepository,
    IDiffCalculator diffCalculator,
    ICurrentUserContext currentUserContext,
    ILogger<DownloadLatestArkReportCommandHandler> logger)
    : IRequestHandler<DownloadLatestArkReportCommand, Result<DownloadLatestArkReportResponse>>
{
    public async Task<Result<DownloadLatestArkReportResponse>> Handle(
        DownloadLatestArkReportCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUserContext.Role != Popocatepetl.Domain.Enums.UserRole.Admin)
        {
            logger.LogWarning("Unauthorized download attempt by {Email}", currentUserContext.Email);
            return Result<DownloadLatestArkReportResponse>.Failure("Only admins can download reports.");
        }

        try
        {
            var downloadedReportContent = await arkReportClient.DownloadLatestAsync(cancellationToken);
            var previousLatest = await reportRepository.GetLatestAsync();
            var fileName = $"arkk_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            if (previousLatest is null)
                logger.LogInformation("No previous report found — diff will not be calculated");

            var report = Report.Create(
                fileName,
                currentUserContext.Email,
                downloadedReportContent,
                isLatest: false);

            await reportRepository.SaveAsync(report);
            await reportRepository.SetLatestAsync(report.Id);

            if (previousLatest is not null)
            {
                var diff = diffCalculator.Calculate(previousLatest, report);
                await diffResultRepository.AddAsync(diff);
            }

            logger.LogInformation("Successfully downloaded the latest ARK report");
            return Result<DownloadLatestArkReportResponse>.Success(new DownloadLatestArkReportResponse(
                report.Id,
                report.FileName,
                report.UploadedAt,
                previousLatest is not null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading and storing the latest ARK report");
            return Result<DownloadLatestArkReportResponse>.Failure(
                $"Failed to download and store the latest report: {ex.Message}");
        }
    }
}
