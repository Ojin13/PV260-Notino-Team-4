using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Users;

/// <summary>Handles CreateUserCommand.</summary>
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    ILogger<CreateUserCommandHandler> logger) : IRequestHandler<CreateUserCommand, AppUser>
{
    public async Task<AppUser> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = AppUser.Create(request.Email, DateTime.UtcNow);
        await userRepository.AddAsync(user);
        logger.LogInformation("Created user {Email}", user.Email);
        return user;
    }
}
