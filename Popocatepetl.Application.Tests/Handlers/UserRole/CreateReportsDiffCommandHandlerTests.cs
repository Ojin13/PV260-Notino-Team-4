using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Reports;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Tests.TestUtilities;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.Handlers.UserRole;

public sealed class CreateReportsDiffCommandHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly Mock<IReportRepository> _reportRepository;
    private readonly Mock<IDiffResultRepository> _diffResultRepository;
    private readonly Mock<IDiffCalculator> _diffCalculator;

    public CreateReportsDiffCommandHandlerTests()
    {
        var builder = new MockedIocBuilder();
        builder
            .AddApplication()
            .AddMocked(out _reportRepository)
            .AddMocked(out _diffResultRepository)
            .AddMocked(out _diffCalculator)
            .AddMocked(out Mock<IAuditLogRepository> _)
            .AddMocked(out Mock<ICurrentUserContext> currentUserContext);

        currentUserContext.SetupGet(x => x.Email).Returns("power@example.com");
        currentUserContext.SetupGet(x => x.Role).Returns(Popocatepetl.Domain.Enums.UserRole.PowerUser);

        _serviceProvider = builder.Build();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Handle_WhenBothReportsExist_CalculatesAndPersistsDiff()
    {
        var baseline = Report.Create("baseline.csv", "seed@example.com", "baseline", false);
        var current = Report.Create("current.csv", "seed@example.com", "current", true);
        var diff = DiffResult.Create(baseline.Id, current.Id);

        _reportRepository.Setup(x => x.GetByIdAsync(baseline.Id)).ReturnsAsync(baseline);
        _reportRepository.Setup(x => x.GetByIdAsync(current.Id)).ReturnsAsync(current);
        _diffCalculator.Setup(x => x.Calculate(baseline, current)).Returns(diff);

        await _mediator.Send(new CreateReportsDiffCommand(baseline.Id, current.Id));

        _diffCalculator.Verify(x => x.Calculate(baseline, current), Times.Once);
        _diffResultRepository.Verify(x => x.AddAsync(diff), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenBaselineIsMissing_ThrowsNotFound()
    {
        var baselineId = Guid.NewGuid();
        var currentId = Guid.NewGuid();
        _reportRepository.Setup(x => x.GetByIdAsync(baselineId)).ReturnsAsync((Report?)null);

        var act = async () => await _mediator.Send(new CreateReportsDiffCommand(baselineId, currentId));

        await act.Should().ThrowExactlyAsync<NotFoundException>();
        _diffResultRepository.Verify(x => x.AddAsync(It.IsAny<DiffResult>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCurrentIsMissing_ThrowsNotFound()
    {
        var baseline = Report.Create("baseline.csv", "seed@example.com", "baseline", false);
        var currentId = Guid.NewGuid();

        _reportRepository.Setup(x => x.GetByIdAsync(baseline.Id)).ReturnsAsync(baseline);
        _reportRepository.Setup(x => x.GetByIdAsync(currentId)).ReturnsAsync((Report?)null);

        var act = async () => await _mediator.Send(new CreateReportsDiffCommand(baseline.Id, currentId));

        await act.Should().ThrowExactlyAsync<NotFoundException>();
        _diffCalculator.Verify(x => x.Calculate(It.IsAny<Report>(), It.IsAny<Report>()), Times.Never);
        _diffResultRepository.Verify(x => x.AddAsync(It.IsAny<DiffResult>()), Times.Never);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
