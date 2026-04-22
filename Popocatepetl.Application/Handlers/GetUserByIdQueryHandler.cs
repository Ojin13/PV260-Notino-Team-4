using MediatR;
using Popocatepetl.Application.Queries;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers;

/// <summary>Handles GetUserByIdQuery.</summary>
public sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, AppUser?>
{
    public Task<AppUser?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        => userRepository.GetByIdAsync(request.Id);
}
