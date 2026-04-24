using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Commands;

public record CreateReportsDiffCommand(
    Guid BaselineReportId,
    Guid CurrentReportId,
    string Email
) : IAuditableRequest, IRequest<Unit>
{
    public string ActionName => "CreateReportsDiff";
    public string Detail => $"Baseline: {BaselineReportId}, Current: {CurrentReportId}";
}