using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Infrastructure.Services;

/// <summary>Fetches ARK report data and converts it to the internal CSV format.</summary>
public sealed class ArkReportClient(
    HttpClient httpClient,
    IOptions<ArkReportOptions> options,
    ILogger<ArkReportClient> logger) : IArkReportClient
{
    private readonly string latestHoldingsUrl = options.Value.LatestHoldingsUrl;

    public async Task<string> DownloadLatestAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Downloading ARK report from {Url}", latestHoldingsUrl);

        using var response = await httpClient.GetAsync(latestHoldingsUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            logger.LogError("ARK report endpoint returned an empty response");
            throw new InvalidOperationException("The ARK report endpoint returned an empty response.");
        }

        return ProjectToCompactCsv(rawContent);
    }

    private string ProjectToCompactCsv(string rawContent)
    {
        using var stringReader = new StringReader(rawContent);
        var readerConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var csvReader = new CsvReader(stringReader, readerConfig);
        csvReader.Context.RegisterClassMap<ArkRowMap>();

        var records = new List<CompactReportRow>();
        var skipped = 0;
        foreach (var row in csvReader.GetRecords<ArkRow>())
        {
            if (string.IsNullOrWhiteSpace(row.Ticker) ||
                string.IsNullOrWhiteSpace(row.Company) ||
                !TryParseShares(row.Shares, out var shares) ||
                !TryParseWeightPercent(row.WeightPercent, out var weightPercent))
            {
                skipped++;
                logger.LogWarning("Skipping ARK row with unparseable data: Ticker={Ticker} Company={Company} Shares={Shares} Weight={Weight}",
                    row.Ticker, row.Company, row.Shares, row.WeightPercent);
                continue;
            }

            records.Add(new CompactReportRow(
                row.Ticker.Trim(),
                row.Company.Trim(),
                shares,
                weightPercent));
        }

        if (records.Count == 0)
        {
            logger.LogError("ARK report yielded no parseable holdings rows; {Skipped} rows were skipped", skipped);
            throw new InvalidOperationException("The ARK report did not contain any holdings rows that could be converted.");
        }

        logger.LogInformation("Parsed {Count} ARK holdings rows, skipped {Skipped}", records.Count, skipped);

        using var output = new StringWriter(CultureInfo.InvariantCulture);
        var writerConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var csvWriter = new CsvWriter(output, writerConfig);
        csvWriter.WriteHeader<CompactReportRow>();
        csvWriter.NextRecord();
        foreach (var record in records)
        {
            csvWriter.WriteRecord(record);
            csvWriter.NextRecord();
        }

        return output.ToString();
    }

    private sealed class ArkRow
    {
        public string Company { get; set; } = string.Empty;
        public string Ticker { get; set; } = string.Empty;
        public string Shares { get; set; } = string.Empty;
        public string WeightPercent { get; set; } = string.Empty;
    }

    private sealed class ArkRowMap : ClassMap<ArkRow>
    {
        public ArkRowMap()
        {
            Map(x => x.Company).Name("company", "Company", "name", "Name");
            Map(x => x.Ticker).Name("ticker", "Ticker");
            Map(x => x.Shares).Name("shares", "Shares");
            Map(x => x.WeightPercent).Name("weight (%)", "WeightPercent", "Weight Percent");
        }
    }

    private static bool TryParseShares(string rawValue, out int shares)
    {
        var normalized = rawValue.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
        return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out shares);
    }

    private static bool TryParseWeightPercent(string rawValue, out double weightPercent)
    {
        var normalized = rawValue.Replace("%", string.Empty, StringComparison.Ordinal).Trim();
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out weightPercent);
    }

    private sealed record CompactReportRow(
        string Ticker,
        string Name,
        int Shares,
        double WeightPercent);
}
