using MediatR;

namespace Popocatepetl.Application.Commands;

/// <summary>Updates the email of the user identified by Id.</summary>
public record UpdateUserCommand(Guid Id, string Email) : IRequest;
