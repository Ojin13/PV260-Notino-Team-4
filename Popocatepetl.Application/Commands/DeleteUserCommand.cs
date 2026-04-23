using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Commands;

public record DeleteUserCommand(Guid Id) : IRequest, IAuditableRequest
{
    public string ActionName => "DeleteUser";
}
