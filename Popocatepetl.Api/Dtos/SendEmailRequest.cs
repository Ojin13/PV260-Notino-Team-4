using System.ComponentModel.DataAnnotations;
using Popocatepetl.Api.Validation;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Api.Dtos;

/// <summary>Request body for sending the latest diff by email.</summary>
public sealed record SendEmailRequest
{
    /// <summary>Email addresses of the recipients. At least one well-formed address is required.</summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one recipient is required.")]
    [EmailList]
    public List<string> Recipients { get; init; } = [];

    /// <summary>Export format for the diff attachment.</summary>
    [Required]
    public DiffExportFormat Format { get; init; }
}