using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Email;

/// <summary>Handles SendEmailCommand: fetches the latest diff, renders it, and emails it as an attachment.</summary>
public sealed class SendEmailCommandHandler(
    IDiffResultRepository diffResultRepository,
    IDiffExporter diffExporter,
    IEmailService emailService,
    ILogger<SendEmailCommandHandler> logger) : IRequestHandler<SendEmailCommand, Result<Unit>>
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<Result<Unit>> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        if (request.Recipients is null || request.Recipients.Count == 0)
            throw new ArgumentException("At least one recipient is required.", nameof(request));

        var invalid = request.Recipients
            .Where(r => string.IsNullOrWhiteSpace(r) || !EmailValidator.IsValid(r))
            .ToList();

        if (invalid.Count > 0)
            throw new ArgumentException(
                $"Invalid email address(es): {string.Join(", ", invalid)}.", nameof(request));

        var diff = await diffResultRepository.GetLastAsync()
            ?? throw new NotFoundException(nameof(DiffResult), "latest");

        var attachment = BuildAttachment(diff, request.Format);
        var timestamp = diff.GeneratedAt.ToString("yyyy-MM-dd HH:mm");

        try
        {
            await emailService.SendAsync(
                subject: $"ARKK holdings diff — {timestamp} UTC",
                body: $"Attached is the latest ARKK holdings diff generated at {timestamp} UTC.",
                recipients: request.Recipients,
                attachment: attachment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", request.Recipients));
            return Result<Unit>.Failure("Email delivery failed. Please check your configuration and try again.");
        }

        logger.LogInformation("Email sent to {Recipients}", string.Join(", ", request.Recipients));
        return Result<Unit>.Success(Unit.Value);
    }

    private MailAttachment BuildAttachment(DiffResult diff, DiffExportFormat format)
    {
        var date = diff.GeneratedAt.ToString("yyyyMMdd");
        return format switch
        {
            DiffExportFormat.Csv => new MailAttachment(
                FileName: $"ARKK_diff_{date}.csv",
                Content: diffExporter.ToCsv(diff, diff.DiffDataEntries),
                ContentType: "text/csv"),
            DiffExportFormat.Pdf => new MailAttachment(
                FileName: $"ARKK_diff_{date}.pdf",
                Content: diffExporter.ToPdf(diff, diff.DiffDataEntries),
                ContentType: "application/pdf"),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format."),
        };
    }
}
