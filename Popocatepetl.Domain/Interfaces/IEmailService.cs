namespace Popocatepetl.Domain.Interfaces;

/// <summary>Contract for sending outbound emails.</summary>
public interface IEmailService
{
    /// <summary>Sends an email with the given subject and body to all recipients.</summary>
    Task SendAsync(string subject, string body, IEnumerable<string> recipients);
}
