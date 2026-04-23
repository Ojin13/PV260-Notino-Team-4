using System.Text;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Infrastructure.Export;
using QuestPDF.Infrastructure;

namespace Popocatepetl.Infrastructure.Tests.Export;

public class DiffExporterTests
{
    static DiffExporterTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly DiffExporter _sut = new();

    private static readonly DiffResult Diff = new()
    {
        BaselineReportId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        CurrentReportId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        GeneratedAt = new DateTime(2026, 04, 23, 10, 0, 0, DateTimeKind.Utc),
    };

    private static readonly List<DiffData> Rows =
    [
        new() { Id = Guid.NewGuid(), Name = "Tesla Inc", Ticker = "TSLA", Shares = 1_000_000,
                SharesDiffPercent = 2.5, ShareDiffType = ShareDiffType.Increased, WeightPercent = 9.87 },
        new() { Id = Guid.NewGuid(), Name = "Roku Inc", Ticker = "ROKU", Shares = 500_000,
                SharesDiffPercent = -1.3, ShareDiffType = ShareDiffType.Decreased, WeightPercent = 3.45 },
    ];

    [Fact]
    public void ToCsv_WritesHeaderAndAllRows()
    {
        var bytes = _sut.ToCsv(Diff, Rows);

        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        lines.Should().HaveCount(3);
        lines[0].Should().Be("Ticker,Name,Shares,SharesDiffPercent,ShareDiffType,WeightPercent");
        lines[1].Should().Be("TSLA,Tesla Inc,1000000,2.5,Increased,9.87");
        lines[2].Should().Be("ROKU,Roku Inc,500000,-1.3,Decreased,3.45");
    }

    [Fact]
    public void ToCsv_WithNoRows_WritesOnlyHeader()
    {
        var bytes = _sut.ToCsv(Diff, []);

        var csv = Encoding.UTF8.GetString(bytes);

        csv.TrimEnd().Should().Be("Ticker,Name,Shares,SharesDiffPercent,ShareDiffType,WeightPercent");
    }

    [Fact]
    public void ToPdf_ReturnsNonEmptyPdfBytes()
    {
        var bytes = _sut.ToPdf(Diff, Rows);

        bytes.Should().NotBeEmpty();
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public void ToPdf_WithNoRows_StillProducesValidPdf()
    {
        var bytes = _sut.ToPdf(Diff, []);

        bytes.Should().NotBeEmpty();
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }
}