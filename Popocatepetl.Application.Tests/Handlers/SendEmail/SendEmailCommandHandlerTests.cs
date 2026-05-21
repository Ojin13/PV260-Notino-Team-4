using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Tests.TestUtilities;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Exceptions;

namespace Popocatepetl.Application.Tests.Handlers.SendEmail;

public class SendEmailCommandHandlerTests : HandlerTestBase
{
    private static readonly IReadOnlyList<string> Recipients = ["alice@example.com", "bob@example.com"];

    private static readonly IReadOnlyList<DiffData> SampleRows =
    [
        DiffData.Create("Tesla", "TSLA", 1_000_000, 2.5, ShareDiffType.Increased, 9.87),
    ];

    private static readonly DiffResult SampleDiff = DiffResult.Create(
        baselineReportId: Guid.NewGuid(),
        currentReportId: Guid.NewGuid(),
        generatedAt: new DateTime(2026, 04, 23, 10, 0, 0, DateTimeKind.Utc),
        diffDataEntries: SampleRows);

    private void SetupLatestDiff() => DiffResultRepository
        .Setup(r => r.GetLastAsync())
        .ReturnsAsync(SampleDiff);

    [Fact]
    public async Task Handle_CsvFormat_SendsEmailWithCsvAttachment()
    {
        SetupLatestDiff();
        var csvBytes = new byte[] { 1, 2, 3 };
        DiffExporter.Setup(e => e.ToCsv(SampleDiff, SampleDiff.DiffDataEntries)).Returns(csvBytes);

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
        DiffExporter.Setup(e => e.ToPdf(SampleDiff, SampleDiff.DiffDataEntries)).Returns(pdfBytes);

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
        DiffResultRepository
            .Setup(r => r.GetLastAsync())
            .ReturnsAsync((DiffResult?)null);

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
        DiffResultRepository.Verify(r => r.GetLastAsync(), Times.Never);
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
        DiffResultRepository.Verify(r => r.GetLastAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrows_ReturnsFailureResult()
    {
        SetupLatestDiff();
        DiffExporter.Setup(e => e.ToCsv(SampleDiff, SampleDiff.DiffDataEntries)).Returns([]);
        EmailService
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<MailAttachment?>()))
            .ThrowsAsync(new Exception("Connection refused"));

        var result = await Mediator.Send(new SendEmailCommand(Recipients, DiffExportFormat.Csv));

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}