using MediatR;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Users;

/// <summary>Handles DeleteUserCommand.</summary>
public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        => await userRepository.DeleteAsync(request.Id);
}
