using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Reports;

public record GetDiffReportQuery() : IAuditableRequest, IRequest<IReadOnlyList<DiffData>?>
{
    public string ActionName => "ShowLatestDiff";

    public string Detail => "";
}
