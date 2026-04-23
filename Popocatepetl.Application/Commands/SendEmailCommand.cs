using MediatR;

namespace Popocatepetl.Application.Commands;

/// <summary>Sends a plain-text email with the given subject and body to the specified recipients.</summary>
public record SendEmailCommand(
    string Subject,
    string Body,
    IReadOnlyList<string> Recipients) : IRequest<Unit>;