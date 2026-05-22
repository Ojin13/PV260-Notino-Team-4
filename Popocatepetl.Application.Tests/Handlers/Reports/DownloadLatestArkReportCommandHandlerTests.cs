using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Reports;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Tests.TestUtilities;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.Handlers.Reports;

public sealed class DownloadLatestArkReportCommandHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly Mock<IArkReportClient> _arkReportClient;
    private readonly Mock<IReportRepository> _reportRepository;
    private readonly Mock<IDiffResultRepository> _diffResultRepository;
    private readonly Mock<IDiffCalculator> _diffCalculator;
    private readonly Mock<IAuditLogRepository> _auditLogRepository;
    private readonly Mock<ICurrentUserContext> _currentUserContext;

    public DownloadLatestArkReportCommandHandlerTests()
    {
        var builder = new MockedIocBuilder();
        builder
            .AddApplication()
            .AddMocked(out _arkReportClient)
            .AddMocked(out _reportRepository)
            .AddMocked(out _diffResultRepository)
            .AddMocked(out _diffCalculator)
            .AddMocked(out _auditLogRepository)
            .AddMocked(out _currentUserContext);

        _serviceProvider = builder.Build();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _currentUserContext.SetupGet(x => x.Email).Returns("admin@example.com");
        _currentUserContext.SetupGet(x => x.Role).Returns(Popocatepetl.Domain.Enums.UserRole.Admin);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotAdmin_ReturnsFailureAndSkipsDownload()
    {
        _currentUserContext.SetupGet(x => x.Role).Returns(Popocatepetl.Domain.Enums.UserRole.User);

        var result = await _mediator.Send(new DownloadLatestArkReportCommand());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Only admins can download reports.");
        _arkReportClient.Verify(x => x.DownloadLatestAsync(It.IsAny<CancellationToken>()), Times.Never);
        _reportRepository.Verify(x => x.SaveAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoPreviousReport_StoresLatestWithoutCreatingDiff()
    {
        const string csv = "Ticker,Name,Shares,WeightPercent\nTSLA,Tesla,10,1.5\n";
        _arkReportClient.Setup(x => x.DownloadLatestAsync(It.IsAny<CancellationToken>())).ReturnsAsync(csv);
        _reportRepository.Setup(x => x.GetLatestAsync()).ReturnsAsync((Report?)null);

        var result = await _mediator.Send(new DownloadLatestArkReportCommand());

        result.IsSuccess.Should().BeTrue();
        result.Value!.PreviousReportFound.Should().BeFalse();
        _reportRepository.Verify(
            x => x.SaveAsync(It.Is<Report>(r =>
                r.UploadedByEmail == "admin@example.com" &&
                r.RawContent == csv &&
                r.IsLatest == false)),
            Times.Once);
        _reportRepository.Verify(x => x.SetLatestAsync(It.IsAny<Guid>()), Times.Once);
        _diffCalculator.Verify(x => x.Calculate(It.IsAny<Report>(), It.IsAny<Report>()), Times.Never);
        _diffResultRepository.Verify(x => x.AddAsync(It.IsAny<DiffResult>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPreviousReportExists_CalculatesAndStoresDiff()
    {
        const string csv = "Ticker,Name,Shares,WeightPercent\nTSLA,Tesla,12,2.0\n";
        var previous = Report.Create(
            "baseline.csv",
            "seed@example.com",
            "Ticker,Name,Shares,WeightPercent\nTSLA,Tesla,10,1.5\n",
            false);
        var calculatedDiff = DiffResult.Create(previous.Id, Guid.NewGuid());

        _arkReportClient.Setup(x => x.DownloadLatestAsync(It.IsAny<CancellationToken>())).ReturnsAsync(csv);
        _reportRepository.Setup(x => x.GetLatestAsync()).ReturnsAsync(previous);
        _diffCalculator
            .Setup(x => x.Calculate(previous, It.IsAny<Report>()))
            .Returns(calculatedDiff);

        var result = await _mediator.Send(new DownloadLatestArkReportCommand());

        result.IsSuccess.Should().BeTrue();
        result.Value!.PreviousReportFound.Should().BeTrue();
        _diffCalculator.Verify(x => x.Calculate(previous, It.IsAny<Report>()), Times.Once);
        _diffResultRepository.Verify(x => x.AddAsync(calculatedDiff), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDownloadThrows_ReturnsFailure()
    {
        _arkReportClient
            .Setup(x => x.DownloadLatestAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var result = await _mediator.Send(new DownloadLatestArkReportCommand());

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to download and store the latest report");
        result.ErrorMessage.Should().Contain("boom");
        _reportRepository.Verify(x => x.SaveAsync(It.IsAny<Report>()), Times.Never);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
