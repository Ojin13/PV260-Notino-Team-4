using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Popocatepetl.Api.Tests.TestUtilities;
using Popocatepetl.Application.Dtos;

namespace Popocatepetl.Api.Tests.Integration;

public sealed class AdminReportsControllerIntegrationTests
{
    [Fact]
    public async Task DownloadLatest_WhenCallerIsAdmin_PersistsReportDiffAndAuditLog()
    {
        await using var factory = new ApiWebApplicationFactory
        {
            LatestArkCsv = """
                Ticker,Name,Shares,WeightPercent
                AAPL,Apple,120,11.1
                NVDA,Nvidia,30,4.2
                """
        };
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Email", "admin@example.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "Admin");

        var response = await client.PostAsync("/api/admin/reports/download-latest", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<DownloadLatestArkReportResponse>();
        payload.Should().NotBeNull();
        payload!.PreviousReportFound.Should().BeTrue();

        var snapshot = await factory.WithDbContextAsync(async db => new
        {
            Reports = await db.Reports.OrderBy(x => x.UploadedAt).ToListAsync(),
            Diffs = await db.DiffResults.Include(x => x.DiffDataEntries).ToListAsync(),
            AuditLogs = await db.AuditLogs.Where(x => x.Action == "DownloadLatestReport").ToListAsync()
        });

        snapshot.Reports.Should().HaveCount(3);
        snapshot.Reports.Should().ContainSingle(x => x.Id == payload.ReportId && x.IsLatest);
        snapshot.Diffs.Should().HaveCount(2);
        snapshot.Diffs.Should().Contain(x =>
            x.CurrentReportId == payload.ReportId &&
            x.DiffDataEntries.Any(entry =>
                entry.Ticker == "MSFT" &&
                entry.ShareDiffType == Popocatepetl.Domain.Enums.ShareDiffType.Decreased &&
                entry.Shares == -75));
        snapshot.AuditLogs.Should().ContainSingle(x => x.UserEmail == "admin@example.com" && x.WasSuccessful);
    }

    [Fact]
    public async Task DownloadLatest_WhenCallerIsNotAdmin_ReturnsForbiddenAndDoesNotPersist()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Email", "user@example.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "User");
        var initialReportCount = await factory.WithDbContextAsync(db => db.Reports.CountAsync());

        var response = await client.PostAsync("/api/admin/reports/download-latest", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var reportCount = await factory.WithDbContextAsync(db => db.Reports.CountAsync());
        reportCount.Should().Be(initialReportCount);
    }
}
