using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries;

/// <summary>Returns the user with the given Id, or null if not found.</summary>
public record GetUserByIdQuery(Guid Id) : IRequest<AppUser?>;
