using MediatR;
using Popocatepetl.Application.Queries.UserRole;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.UserRole
{
    internal class GetReportDiffHandler(IDiffResultRepository diffRepository) : IRequestHandler<GetDiffReportQuery, IReadOnlyList<DiffData>?>
    {
        public async Task<IReadOnlyList<DiffData>?> Handle(GetDiffReportQuery request, CancellationToken cancellationToken)
        {
            return (await diffRepository.GetLastAsync())?.DiffDataEntries.ToList();
        }
    }
}
