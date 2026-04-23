using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Tests.TestUtilities;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.Handlers.SendEmail;

public class SendEmailCommandHandlerTests : HandlerTestBase
{
    private static readonly IReadOnlyList<string> Recipients = ["alice@example.com", "bob@example.com"];

    private static readonly DiffResult SampleDiff = new()
    {
        BaselineReportId = Guid.NewGuid(),
        CurrentReportId = Guid.NewGuid(),
        GeneratedAt = new DateTime(2026, 04, 23, 10, 0, 0, DateTimeKind.Utc),
    };

    private static readonly IReadOnlyList<DiffData> SampleRows =
    [
        new() { Id = Guid.NewGuid(), Name = "Tesla", Ticker = "TSLA", Shares = 1_000_000,
                SharesDiffPercent = 2.5, ShareDiffType = ShareDiffType.Increased, WeightPercent = 9.87 },
    ];

    private void SetupLatestDiff() => DiffRepository
        .Setup(r => r.GetLatestAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new LatestDiff(SampleDiff, SampleRows));

    [Fact]
    public async Task Handle_CsvFormat_SendsEmailWithCsvAttachment()
    {
        SetupLatestDiff();
        var csvBytes = new byte[] { 1, 2, 3 };
        DiffExporter.Setup(e => e.ToCsv(SampleDiff, SampleRows)).Returns(csvBytes);

        await Mediator.Send(new SendEmailCommand(Recipients, DiffExportFormat.Csv));

        EmailService.Verify(s => s.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            Recipients,
            It.Is<MailAttachment>(a =>
                a.FileName == "ARKK_diff_20260423.csv" &&
                a.ContentType == "text/csv" &&
                a.Content == csvBytes)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PdfFormat_SendsEmailWithPdfAttachment()
    {
        SetupLatestDiff();
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"
        DiffExporter.Setup(e => e.ToPdf(SampleDiff, SampleRows)).Returns(pdfBytes);

        await Mediator.Send(new SendEmailCommand(Recipients, DiffExportFormat.Pdf));

        EmailService.Verify(s => s.SendAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            Recipients,
            It.Is<MailAttachment>(a =>
                a.FileName == "ARKK_diff_20260423.pdf" &&
                a.ContentType == "application/pdf" &&
                a.Content == pdfBytes)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoDiffExists_ThrowsNotFoundAndDoesNotSend()
    {
        DiffRepository
            .Setup(r => r.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((LatestDiff?)null);

        var act = async () => await Mediator.Send(new SendEmailCommand(Recipients, DiffExportFormat.Csv));

        await act.Should().ThrowAsync<NotFoundException>();
        EmailService.Verify(s => s.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IEnumerable<string>>(), It.IsAny<MailAttachment?>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyRecipients_ThrowsAndDoesNotTouchRepository()
    {
        var act = async () => await Mediator.Send(new SendEmailCommand([], DiffExportFormat.Csv));

        await act.Should().ThrowAsync<ArgumentException>();
        DiffRepository.Verify(r => r.GetLatestAsync(It.IsAny<CancellationToken>()), Times.Never);
        EmailService.Verify(s => s.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IEnumerable<string>>(), It.IsAny<MailAttachment?>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMalformedRecipient_Throws()
    {
        var act = async () => await Mediator.Send(
            new SendEmailCommand(["alice@example.com", "not-an-email"], DiffExportFormat.Csv));

        (await act.Should().ThrowAsync<ArgumentException>())
            .Which.Message.Should().Contain("not-an-email");
        DiffRepository.Verify(r => r.GetLatestAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}