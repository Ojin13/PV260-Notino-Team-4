using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Contract for sending outbound emails.</summary>
public interface IEmailService
{
    /// <summary>Sends an email with the given subject, body, and an optional single attachment to all recipients.</summary>
    Task SendAsync(
        string subject,
        string body,
        IEnumerable<string> recipients,
        MailAttachment? attachment = null);
}