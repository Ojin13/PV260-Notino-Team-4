using System.ComponentModel.DataAnnotations;
using MediatR;
using Popocatepetl.Application.Commands;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers;

/// <summary>Handles SendEmailCommand by delegating to the configured IEmailService.</summary>
public sealed class SendEmailCommandHandler(
    IEmailService emailService) : IRequestHandler<SendEmailCommand, Unit>
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<Unit> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject must not be empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body must not be empty.", nameof(request));

        if (request.Recipients is null || request.Recipients.Count == 0)
            throw new ArgumentException("At least one recipient is required.", nameof(request));

        var invalid = request.Recipients
            .Where(r => string.IsNullOrWhiteSpace(r) || !EmailValidator.IsValid(r))
            .ToList();

        if (invalid.Count > 0)
            throw new ArgumentException(
                $"Invalid email address(es): {string.Join(", ", invalid)}.", nameof(request));

        await emailService.SendAsync(request.Subject, request.Body, request.Recipients);
        return Unit.Value;
    }
}