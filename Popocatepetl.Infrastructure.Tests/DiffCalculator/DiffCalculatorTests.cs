using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Infrastructure.DiffCalculator;

namespace Popocatepetl.Infrastructure.Tests.DiffCalculator;

public sealed class DiffCalculatorTests
{
    private readonly Popocatepetl.Infrastructure.DiffCalculator.DiffCalculator _sut = new();

    [Fact]
    public void Calculate_WhenTickerIsNew_ReturnsNewRow()
    {
        var baseline = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,100,10
            """);
        var current = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,100,10
            NVDA,Nvidia,25,4.5
            """);

        var result = _sut.Calculate(baseline, current);

        result.DiffDataEntries.Should().ContainSingle(x =>
            x.Ticker == "NVDA" &&
            x.ShareDiffType == ShareDiffType.New &&
            x.Shares == 25 &&
            x.SharesDiffPercent == 100);
    }

    [Fact]
    public void Calculate_WhenSharesIncreaseAndDecrease_ReturnsDirectionalRows()
    {
        var baseline = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,100,10
            MSFT,Microsoft,80,8
            """);
        var current = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,120,11
            MSFT,Microsoft,60,7
            """);

        var result = _sut.Calculate(baseline, current);

        result.DiffDataEntries.Should().Contain(x =>
            x.Ticker == "AAPL" &&
            x.ShareDiffType == ShareDiffType.Increased &&
            x.Shares == 20 &&
            x.SharesDiffPercent == 20);
        result.DiffDataEntries.Should().Contain(x =>
            x.Ticker == "MSFT" &&
            x.ShareDiffType == ShareDiffType.Decreased &&
            x.Shares == -20 &&
            x.SharesDiffPercent == -25);
    }

    [Fact]
    public void Calculate_WhenSharesDoNotChange_SkipsRow()
    {
        var baseline = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,100,10
            """);
        var current = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            AAPL,Apple,100,12
            """);

        var result = _sut.Calculate(baseline, current);

        result.DiffDataEntries.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_WhenTickerDisappears_ReturnsMinusHundredPercentRow()
    {
        var baseline = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            TSLA,Tesla,40,5
            """);
        var current = CreateReport("""
            Ticker,Name,Shares,WeightPercent
            """);

        var result = _sut.Calculate(baseline, current);

        result.DiffDataEntries.Should().ContainSingle(x =>
            x.Ticker == "TSLA" &&
            x.ShareDiffType == ShareDiffType.Decreased &&
            x.Shares == -40 &&
            x.SharesDiffPercent == -100 &&
            x.WeightPercent == 0);
    }

    [Fact]
    public void Calculate_WhenTickerIsMissing_UsesCusipAsSecurityKey()
    {
        var baseline = CreateReport("""
            Company,Ticker,CUSIP,Shares,WeightPercent
            Some Corp,,CUSIP123,100,5
            """);
        var current = CreateReport("""
            Company,Ticker,CUSIP,Shares,WeightPercent
            Some Corp,,CUSIP123,150,6
            """);

        var result = _sut.Calculate(baseline, current);

        result.DiffDataEntries.Should().ContainSingle(x =>
            x.Name == "Some Corp" &&
            x.Ticker == string.Empty &&
            x.ShareDiffType == ShareDiffType.Increased &&
            x.Shares == 50 &&
            x.SharesDiffPercent == 50);
    }

    private static Report CreateReport(string rawContent)
        => Report.Create(
            "report.csv",
            "tester@example.com",
            rawContent.ReplaceLineEndings("\n"),
            false);
}
