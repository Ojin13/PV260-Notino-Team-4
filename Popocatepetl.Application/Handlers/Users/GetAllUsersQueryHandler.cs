using MediatR;
using Popocatepetl.Application.Queries.Users;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.Users;

/// <summary>Handles GetAllUsersQuery.</summary>
public sealed class GetAllUsersQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, IReadOnlyList<AppUser>>
{
    public async Task<IReadOnlyList<AppUser>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        => (await userRepository.GetAllAsync()).ToList().AsReadOnly();
}
