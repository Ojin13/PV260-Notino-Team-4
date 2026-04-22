using MediatR;

namespace Popocatepetl.Application.Commands;

/// <summary>Deletes the user identified by Id.</summary>
public record DeleteUserCommand(Guid Id) : IRequest;
