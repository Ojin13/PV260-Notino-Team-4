using MediatR;
using Popocatepetl.Application.Commands;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers;

public sealed class CreateReportsDiffCommandHandler(
    IDiffResultRepository diffResultRepository,
    Guid baselineReportId,
    Guid currentReportId
) : IRequestHandler<CreateReportsDiffCommand, Unit>
{
    public Task<Unit> Handle(CreateReportsDiffCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}