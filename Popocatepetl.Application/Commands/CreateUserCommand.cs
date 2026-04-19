using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Commands;

/// <summary>Creates a new user with the given Email.</summary>
public record CreateUserCommand(string Email) : IRequest<AppUser>;
