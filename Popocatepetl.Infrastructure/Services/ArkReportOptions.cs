namespace Popocatepetl.Infrastructure.Services;

/// <summary>ARK report download settings.</summary>
public sealed class ArkReportOptions
{
    public const string SectionName = "ArkReports";

    public string LatestHoldingsUrl { get; set; } =
        "https://assets.ark-funds.com/fund-documents/funds-etf-csv/ARK_INNOVATION_ETF_ARKK_HOLDINGS.csv";
}
