using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Resend;

namespace Popocatepetl.Infrastructure.Email;

/// <summary>IEmailService implementation backed by the Resend API.</summary>
public sealed class ResendEmailService(
    IResend resend,
    IOptions<ResendOptions> options,
    ILogger<ResendEmailService> logger) : IEmailService
{
    private readonly ResendOptions _options = options.Value;

    public async Task SendAsync(
        string subject,
        string body,
        IEnumerable<string> recipients,
        MailAttachment? attachment = null)
    {
        logger.LogInformation("Sending email from {FromAddress} to {recipients}", _options.FromAddress, string.Join(", ", recipients));
        var message = new EmailMessage
        {
            From = _options.FromAddress,
            Subject = subject,
            TextBody = body,
        };

        foreach (var recipient in recipients)
            message.To.Add(recipient);

        if (attachment is not null)
        {
            message.Attachments ??= [];
            message.Attachments.Add(new EmailAttachment
            {
                Filename = attachment.FileName,
                Content = attachment.Content,
                ContentType = attachment.ContentType,
            });
        }

        await resend.EmailSendAsync(message);
        logger.LogInformation("Email sent from {FromAddress} to {Recipients}{AttachmentNote}",
            _options.FromAddress,
            string.Join(", ", recipients),
            attachment is not null ? $" with attachment {attachment.FileName}" : string.Empty);
    }
}