using MediatR;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Users;

/// <summary>Handles UpdateUserCommand.</summary>
public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException(nameof(AppUser), request.Id);

        user.Email = request.Email;
        await userRepository.UpdateAsync(user);
    }
}
