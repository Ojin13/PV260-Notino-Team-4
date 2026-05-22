using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Users;

/// <summary>Returns all registered users.</summary>
public record GetAllUsersQuery : IRequest<IReadOnlyList<AppUser>>;
