using MediatR;

namespace Popocatepetl.Application.Commands;

public record CreateReportsDiffCommand(
    Guid BaselineReportId,
    Guid CurrentReportId
) : IRequest<Unit>;