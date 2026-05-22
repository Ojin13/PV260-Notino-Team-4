using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Infrastructure.DiffCalculator;

public sealed class DiffCalculator(ILogger<DiffCalculator> logger) : IDiffCalculator
{
    public DiffResult Calculate(Report baseline, Report current)
    {
        var baselineRows = ParseRows(baseline.RawContent, "baseline");
        var currentRows = ParseRows(current.RawContent, "current");

        return DiffResult.CreateFromHoldings(baseline.Id, current.Id, baselineRows, currentRows);
    }

    private IReadOnlyDictionary<string, HoldingSnapshot> ParseRows(string csvContent, string label)
    {
        var records = new Dictionary<string, HoldingSnapshot>(StringComparer.OrdinalIgnoreCase);
        var skipped = 0;

        using var stringReader = new StringReader(csvContent);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var csv = new CsvReader(stringReader, config);
        csv.Context.RegisterClassMap<ReportRowCsvMap>();

        try
        {
            foreach (var row in csv.GetRecords<ReportRowCsv>())
            {
                var securityKey = GetSecurityKey(row.Ticker, row.Cusip);
                if (string.IsNullOrWhiteSpace(securityKey))
                {
                    skipped++;
                    continue;
                }

                records[securityKey] = new HoldingSnapshot(
                    row.Ticker,
                    row.Company ?? string.Empty,
                    row.Shares,
                    row.WeightPercent);
            }
        }
        catch (HeaderValidationException ex)
        {
            logger.LogWarning(ex, "CSV header validation failed for {Label} report — treating as empty", label);
            return records;
        }

        if (skipped > 0)
            logger.LogWarning("Skipped {Skipped} rows with no identifiable security key in {Label} report", skipped, label);

        logger.LogInformation("Parsed {Count} holdings from {Label} report", records.Count, label);

        return records;
    }

    private static string GetSecurityKey(string ticker, string cusip)
    {
        return !string.IsNullOrWhiteSpace(ticker) ? ticker : cusip;
    }

    private sealed class ReportRowCsv
    {
        public string Date { get; set; } = string.Empty;
        public string Fund { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Ticker { get; set; } = string.Empty;
        public string Cusip { get; set; } = string.Empty;
        public int Shares { get; set; }
        public decimal MarketValueUsd { get; set; }
        public double WeightPercent { get; set; }
    }

    private sealed class ReportRowCsvMap : ClassMap<ReportRowCsv>
    {
        public ReportRowCsvMap()
        {
            Map(x => x.Date).Name("date", "Date").Optional();
            Map(x => x.Fund).Name("fund", "Fund").Optional();
            Map(x => x.Company).Name("company", "Company", "name", "Name");
            Map(x => x.Ticker).Name("ticker", "Ticker").Optional();
            Map(x => x.Cusip).Name("cusip", "Cusip", "CUSIP").Optional();
            Map(x => x.Shares).Name("shares", "Shares");
            Map(x => x.MarketValueUsd).Name("market value ($)", "MarketValueUsd", "Market Value USD").Optional();
            Map(x => x.WeightPercent).Name("weight (%)", "WeightPercent", "Weight Percent");
        }
    }

}
