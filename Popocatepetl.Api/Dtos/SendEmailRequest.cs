using System.ComponentModel.DataAnnotations;
using Popocatepetl.Api.Validation;

namespace Popocatepetl.Api.Dtos;

/// <summary>Request body for sending an email.</summary>
public sealed record SendEmailRequest
{
    /// <summary>Subject line of the email.</summary>
    [Required]
    public string Subject { get; init; } = string.Empty;

    /// <summary>Plain-text body of the email.</summary>
    [Required]
    public string Body { get; init; } = string.Empty;

    /// <summary>Email addresses of the recipients. At least one well-formed address is required.</summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one recipient is required.")]
    [EmailList]
    public List<string> Recipients { get; init; } = [];
}