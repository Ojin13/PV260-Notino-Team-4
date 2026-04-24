using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries.Users;

/// <summary>Returns all registered users.</summary>
public record GetAllUsersQuery : IRequest<IReadOnlyList<AppUser>>;
