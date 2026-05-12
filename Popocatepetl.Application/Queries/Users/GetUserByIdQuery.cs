using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries.Users;

/// <summary>Returns the user with the given Id, or null if not found.</summary>
public record GetUserByIdQuery(Guid Id) : IRequest<AppUser?>;
