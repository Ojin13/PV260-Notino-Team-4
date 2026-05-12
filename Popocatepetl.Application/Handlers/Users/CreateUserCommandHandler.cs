using MediatR;
using Popocatepetl.Application.Commands;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.Users;

/// <summary>Handles CreateUserCommand.</summary>
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository) : IRequestHandler<CreateUserCommand, AppUser>
{
    public async Task<AppUser> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = AppUser.Create(request.Email, DateTime.UtcNow);

        await userRepository.AddAsync(user);
        return user;
    }
}
