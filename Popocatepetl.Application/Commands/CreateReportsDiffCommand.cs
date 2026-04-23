using MediatR;

namespace Popocatepetl.Application.Commands;

public record CreateReportsDiffCommand(
    Guid baselineReportId,
    Guid currentReportId
) : IRequest<Unit>;