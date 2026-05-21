using System.ComponentModel.DataAnnotations;

namespace Popocatepetl.Infrastructure.Email;

/// <summary>Configuration for the Resend email provider.</summary>
public sealed class ResendOptions
{
    public const string SectionName = "Resend";

    /// <summary>Resend API key. Supply via user-secrets or environment variable in real environments.</summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Sender address used for outbound emails (must be a verified Resend domain).</summary>
    [Required]
    public string FromAddress { get; set; } = string.Empty;
}