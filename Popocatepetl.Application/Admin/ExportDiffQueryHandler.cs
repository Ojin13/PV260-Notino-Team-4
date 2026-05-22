using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Admin;

internal sealed class ExportDiffQueryHandler(
    IDiffResultRepository diffResultRepository,
    IDiffExporter diffExporter,
    ILogger<ExportDiffQueryHandler> logger)
    : IRequestHandler<ExportDiffQuery, ExportDiffResponse?>
{
    public async Task<ExportDiffResponse?> Handle(ExportDiffQuery request, CancellationToken cancellationToken)
    {
        var diff = await diffResultRepository.GetLastAsync();
        if (diff is null)
        {
            logger.LogInformation("Export requested but no diff result found.");
            return null;
        }

        logger.LogInformation("Exporting diff {DiffId} as {Format}.", diff.Id, request.Format);

        var ext = request.Format == DiffExportFormat.Pdf ? "pdf" : "csv";
        var suggestedFileName = $"diff-{diff.GeneratedAt:yyyyMMdd-HHmmss}.{ext}";

        var bytes = request.Format == DiffExportFormat.Pdf
            ? diffExporter.ToPdf(diff, diff.DiffDataEntries)
            : diffExporter.ToCsv(diff, diff.DiffDataEntries);

        return new ExportDiffResponse(bytes, suggestedFileName);
    }
}
