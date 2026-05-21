using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Application.Queries.UserRole;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.UserRole
{
    internal class GetDiffReportQueryHandler(IDiffResultRepository diffRepository, ILogger<GetDiffReportQueryHandler> logger) : IRequestHandler<GetDiffReportQuery, IReadOnlyList<DiffData>?>
    {
        public async Task<IReadOnlyList<DiffData>?> Handle(GetDiffReportQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching diff report");
            return (await diffRepository.GetLastAsync())?.DiffDataEntries.ToList();
        }
    }
}
