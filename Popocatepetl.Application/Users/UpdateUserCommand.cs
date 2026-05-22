using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Users;

public record UpdateUserCommand(Guid Id, string Email) : IRequest, IAuditableRequest
{
    public string ActionName => "UpdateUser";
    public string Detail => "User Id: {Id}";
}
