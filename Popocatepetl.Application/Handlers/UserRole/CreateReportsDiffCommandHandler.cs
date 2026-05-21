using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.UserRole;

public sealed class CreateReportsDiffCommandHandler(
    IReportRepository reportRepository,
    IDiffResultRepository diffResultRepository,
    IDiffCalculator diffCalculator,
    ILogger<CreateReportsDiffCommandHandler> logger)
    : IRequestHandler<CreateReportsDiffCommand, Unit>
{
    public async Task<Unit> Handle(CreateReportsDiffCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing create reports diff command");
        var baseline = await reportRepository.GetByIdAsync(request.BaselineReportId)
            ?? throw new NotFoundException(nameof(Report), request.BaselineReportId);

        var current = await reportRepository.GetByIdAsync(request.CurrentReportId)
            ?? throw new NotFoundException(nameof(Report), request.CurrentReportId);

        var diffResult = diffCalculator.Calculate(baseline, current);

        await diffResultRepository.AddAsync(diffResult);
        logger.LogInformation("Successfully created reports diff");
        return Unit.Value;
    }
}