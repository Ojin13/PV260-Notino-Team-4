using MediatR;
using Popocatepetl.Application.Commands;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers;

/// <summary>Handles DeleteUserCommand.</summary>
public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        => await userRepository.DeleteAsync(request.Id);
}
