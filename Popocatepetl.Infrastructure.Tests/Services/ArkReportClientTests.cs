using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Popocatepetl.Infrastructure.Services;

namespace Popocatepetl.Infrastructure.Tests.Services;

public sealed class ArkReportClientTests
{
    [Fact]
    public async Task DownloadLatestAsync_WhenResponseContainsValidRows_ReturnsCompactCsv()
    {
        var sut = CreateSut("""
            company,ticker,shares,weight (%)
            Tesla,TSLA,"1,000",2.50%
            Roku,ROKU,500,1.25%
            """);

        var result = await sut.DownloadLatestAsync(CancellationToken.None);

        result.ReplaceLineEndings("\n").TrimEnd().Should().Be("""
            Ticker,Name,Shares,WeightPercent
            TSLA,Tesla,1000,2.5
            ROKU,Roku,500,1.25
            """.ReplaceLineEndings("\n").TrimEnd());
    }

    [Fact]
    public async Task DownloadLatestAsync_WhenResponseContainsInvalidRows_SkipsThem()
    {
        var sut = CreateSut("""
            company,ticker,shares,weight (%)
            Tesla,TSLA,"1,000",2.50%
            MissingShares,MSH,,4.0%
            MissingTicker,,100,1.0%
            """);

        var result = await sut.DownloadLatestAsync(CancellationToken.None);

        result.Should().Contain("TSLA,Tesla,1000,2.5");
        result.Should().NotContain("MissingShares");
        result.Should().NotContain("MissingTicker");
    }

    [Fact]
    public async Task DownloadLatestAsync_WhenResponseIsEmpty_Throws()
    {
        var sut = CreateSut("   ");

        var act = async () => await sut.DownloadLatestAsync(CancellationToken.None);

        await act.Should().ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("*empty response*");
    }

    [Fact]
    public async Task DownloadLatestAsync_WhenNoRowsCanBeConverted_Throws()
    {
        var sut = CreateSut("""
            company,ticker,shares,weight (%)
            MissingTicker,,100,1.0%
            BadShares,AAA,NaN,4.0%
            """);

        var act = async () => await sut.DownloadLatestAsync(CancellationToken.None);

        await act.Should().ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("*did not contain any holdings rows*");
    }

    private static ArkReportClient CreateSut(string responseBody)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("text/csv") }
            }
        });
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new ArkReportOptions
        {
            LatestHoldingsUrl = "https://example.test/ark.csv"
        });

        return new ArkReportClient(httpClient, options, NullLogger<ArkReportClient>.Instance);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
