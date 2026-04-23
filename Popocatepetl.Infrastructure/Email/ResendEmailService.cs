using Microsoft.Extensions.Options;
using Popocatepetl.Domain.Interfaces;
using Resend;

namespace Popocatepetl.Infrastructure.Email;

/// <summary>IEmailService implementation backed by the Resend API.</summary>
public sealed class ResendEmailService(
    IResend resend,
    IOptions<ResendOptions> options) : IEmailService
{
    private readonly ResendOptions _options = options.Value;

    public async Task SendAsync(string subject, string body, IEnumerable<string> recipients)
    {
        var message = new EmailMessage
        {
            From = _options.FromAddress,
            Subject = subject,
            TextBody = body,
        };

        foreach (var recipient in recipients)
            message.To.Add(recipient);

        await resend.EmailSendAsync(message);
    }
}