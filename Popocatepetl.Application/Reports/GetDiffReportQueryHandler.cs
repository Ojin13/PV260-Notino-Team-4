using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Reports;

internal class GetDiffReportQueryHandler(
    IDiffResultRepository diffRepository,
    ILogger<GetDiffReportQueryHandler> logger) : IRequestHandler<GetDiffReportQuery, IReadOnlyList<DiffData>?>
{
    public async Task<IReadOnlyList<DiffData>?> Handle(GetDiffReportQuery request, CancellationToken cancellationToken)
    {
        var diff = await diffRepository.GetLastAsync();
        if (diff is null)
        {
            logger.LogWarning("No diff result found");
            return null;
        }
        return diff.DiffDataEntries.ToList();
    }
}
