using CsvHelper;
using CsvHelper.Configuration;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Interfaces;
using System.Globalization;

namespace Popocatepetl.Infrastructure.Repositories;

/// <summary>Calculates report diffs from CSV report content using CsvHelper.</summary>
public sealed class DiffCalculator : IDiffCalculator
{
    public DiffResult Calculate(Report baseline, Report current)
    {
        var baselineRows = ParseRows(baseline.RawContent);
        var currentRows = ParseRows(current.RawContent);

        var diffRows = CalculateDiffRows(baselineRows, currentRows);
        return DiffResult.Create(
            baseline.Id,
            current.Id,
            diffDataEntries: diffRows);
    }

    private static IReadOnlyList<DiffData> CalculateDiffRows(
        IReadOnlyDictionary<string, ReportRow> baselineRows,
        IReadOnlyDictionary<string, ReportRow> currentRows)
    {
        var diffs = new List<DiffData>();

        foreach (var (securityKey, current) in currentRows)
        {
            if (!baselineRows.TryGetValue(securityKey, out var baseline))
            {
                diffs.Add(DiffData.Create(
                    current.Name,
                    current.Ticker,
                    current.Shares,
                    100,
                    ShareDiffType.New,
                    current.WeightPercent));
                continue;
            }

            var sharesDelta = current.Shares - baseline.Shares;
            if (sharesDelta == 0)
            {
                continue;
            }

            var diffPercent = baseline.Shares == 0
                ? 100
                : (double)sharesDelta / baseline.Shares * 100;

            diffs.Add(DiffData.Create(
                current.Name,
                current.Ticker,
                sharesDelta,
                diffPercent,
                sharesDelta > 0 ? ShareDiffType.Increased : ShareDiffType.Decreased,
                current.WeightPercent));
        }

        foreach (var (securityKey, baseline) in baselineRows)
        {
            if (currentRows.ContainsKey(securityKey))
            {
                continue;
            }

            diffs.Add(DiffData.Create(
                baseline.Name,
                baseline.Ticker,
                -baseline.Shares,
                -100,
                ShareDiffType.Decreased,
                0));
        }

        return diffs;
    }

    private static IReadOnlyDictionary<string, ReportRow> ParseRows(string csvContent)
    {
        var records = new Dictionary<string, ReportRow>(StringComparer.OrdinalIgnoreCase);

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
                    continue;
                }

                records[securityKey] = new ReportRow(
                    row.Ticker,
                    row.Company ?? string.Empty,
                    row.Shares,
                    row.WeightPercent);
            }
        }
        catch (HeaderValidationException)
        {
            return records;
        }

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

    private sealed record ReportRow(
        string Ticker,
        string Name,
        int Shares,
        double WeightPercent);
}
