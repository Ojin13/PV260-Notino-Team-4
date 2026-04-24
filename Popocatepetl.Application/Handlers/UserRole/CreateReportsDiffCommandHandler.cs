using MediatR;
using Popocatepetl.Application.Commands;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.UserRole;

public sealed class CreateReportsDiffCommandHandler(
    IReportRepository reportRepository,
    IDiffResultRepository diffResultRepository,
    IDiffCalculator diffCalculator
) : IRequestHandler<CreateReportsDiffCommand, Unit>
{
    public async Task<Unit> Handle(CreateReportsDiffCommand request, CancellationToken cancellationToken)
    {
        var baseline = await reportRepository.GetByIdAsync(request.baselineReportId)
            ?? throw new NotFoundException(nameof(Report), request.baselineReportId);

        var current = await reportRepository.GetByIdAsync(request.currentReportId)
            ?? throw new NotFoundException(nameof(Report), request.currentReportId);

        var diffResult = diffCalculator.Calculate(baseline, current);

        await diffResultRepository.AddAsync(diffResult);
        return Unit.Value;
    }
}