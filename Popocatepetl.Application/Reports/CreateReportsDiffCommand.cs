using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Reports;

public record CreateReportsDiffCommand(
    Guid BaselineReportId,
    Guid CurrentReportId
) : IAuditableRequest, IRequest<Unit>
{
    public string ActionName => "CreateReportsDiff";
    public string Detail => $"Baseline: {BaselineReportId}, Current: {CurrentReportId}";
}
