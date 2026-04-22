using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries;

/// <summary>Returns all registered users.</summary>
public record GetAllUsersQuery : IRequest<IReadOnlyList<AppUser>>;
