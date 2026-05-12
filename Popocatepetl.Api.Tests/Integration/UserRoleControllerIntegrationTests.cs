using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Popocatepetl.Api.Controllers;
using Popocatepetl.Api.Tests.TestUtilities;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Api.Tests.Integration;

public sealed class UserRoleControllerIntegrationTests
{
    [Fact]
    public async Task CreateDiffReport_ThenGetDiffReport_WritesAndReadsThroughDatabase()
    {
        await using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient();
        var initialDiffCount = await factory.WithDbContextAsync(db => db.DiffResults.CountAsync());
        var baseline = Popocatepetl.Domain.Entities.Report.Create(
            "baseline.csv",
            "seed@example.com",
            """
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,100,10.5
            MSFT,Microsoft,80,9.2
            """,
            false);
        var current = Popocatepetl.Domain.Entities.Report.Create(
            "current.csv",
            "seed@example.com",
            """
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,120,11.1
            MSFT,Microsoft,75,8.7
            NVDA,Nvidia,30,4.2
            """,
            true);
        await factory.SeedReportAsync(baseline);
        await factory.SeedReportAsync(current);

        var createResponse = await client.PostAsJsonAsync(
            "/api/user-role/diff-report",
            new CreateReportsDiffRequest(baseline.Id, current.Id));

        createResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var diffResponse = await client.GetAsync("/api/user-role/diff-report");

        diffResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await diffResponse.Content.ReadFromJsonAsync<List<DiffRowResponse>>();
        rows.Should().NotBeNull();
        rows!.Should().Contain(x => x.Ticker == "AAPL" && x.Shares == 20);
        rows.Should().Contain(x => x.Ticker == "MSFT" && x.Shares == -5);
        rows.Should().Contain(x => x.Ticker == "NVDA" && x.ShareDiffType == "New");

        var diffCount = await factory.WithDbContextAsync(db => db.DiffResults.CountAsync());
        diffCount.Should().Be(initialDiffCount + 1);
    }

    private sealed record DiffRowResponse(
        string Name,
        string Ticker,
        int Shares,
        double SharesDiffPercent,
        string ShareDiffType,
        double WeightPercent);
}
