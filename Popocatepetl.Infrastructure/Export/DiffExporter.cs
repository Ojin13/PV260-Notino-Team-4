using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Popocatepetl.Infrastructure.Export;

/// <summary>Renders a diff into CSV or PDF byte content.</summary>
public sealed class DiffExporter : IDiffExporter
{
    public byte[] ToCsv(DiffResult diff, IEnumerable<DiffData> rows)
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteField("Ticker");
            csv.WriteField("Name");
            csv.WriteField("Shares");
            csv.WriteField("SharesDiffPercent");
            csv.WriteField("ShareDiffType");
            csv.WriteField("WeightPercent");
            csv.NextRecord();

            foreach (var row in rows)
            {
                csv.WriteField(row.Ticker);
                csv.WriteField(row.Name);
                csv.WriteField(row.Shares);
                csv.WriteField(row.SharesDiffPercent);
                csv.WriteField(row.ShareDiffType.ToString());
                csv.WriteField(row.WeightPercent);
                csv.NextRecord();
            }
        }

        return stream.ToArray();
    }

    public byte[] ToPdf(DiffResult diff, IEnumerable<DiffData> rows)
    {
        var materialized = rows as IReadOnlyList<DiffData> ?? rows.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("ARKK holdings diff").FontSize(16).SemiBold();
                    col.Item().Text($"Generated at {diff.GeneratedAt:yyyy-MM-dd HH:mm} UTC")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);  // Ticker
                        c.RelativeColumn(3);  // Name
                        c.RelativeColumn(1);  // Shares
                        c.RelativeColumn(1);  // SharesDiffPercent
                        c.RelativeColumn(1);  // ShareDiffType
                        c.RelativeColumn(1);  // WeightPercent
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Ticker");
                        header.Cell().Element(HeaderCell).Text("Name");
                        header.Cell().Element(HeaderCell).Text("Shares");
                        header.Cell().Element(HeaderCell).Text("Diff %");
                        header.Cell().Element(HeaderCell).Text("Type");
                        header.Cell().Element(HeaderCell).Text("Weight %");
                    });

                    foreach (var row in materialized)
                    {
                        table.Cell().Element(BodyCell).Text(row.Ticker);
                        table.Cell().Element(BodyCell).Text(row.Name);
                        table.Cell().Element(BodyCell).Text(row.Shares.ToString(CultureInfo.InvariantCulture));
                        table.Cell().Element(BodyCell).Text(row.SharesDiffPercent.ToString("F2", CultureInfo.InvariantCulture));
                        table.Cell().Element(BodyCell).Text(row.ShareDiffType.ToString());
                        table.Cell().Element(BodyCell).Text(row.WeightPercent.ToString("F2", CultureInfo.InvariantCulture));
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();

        static IContainer HeaderCell(IContainer c) =>
            c.DefaultTextStyle(t => t.SemiBold())
             .PaddingVertical(4).PaddingHorizontal(4)
             .Background(Colors.Grey.Lighten3);

        static IContainer BodyCell(IContainer c) =>
            c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
             .PaddingVertical(3).PaddingHorizontal(4);
    }
}