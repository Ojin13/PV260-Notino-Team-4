using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Commands;

public record CreateUserCommand(string Email) : IRequest<AppUser>, IAuditableRequest
{
    public string ActionName => "CreateUser";
}
