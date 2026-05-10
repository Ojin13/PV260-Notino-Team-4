using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries.UserRole
{
    public record GetDiffReportQuery() : IAuditableRequest, IRequest<IReadOnlyList<DiffData>?>
    {
        public string ActionName => "ShowLatestDiff";

        public string Detail => "";
    }
}
