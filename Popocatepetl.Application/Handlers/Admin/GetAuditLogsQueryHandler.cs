using MediatR;
using Popocatepetl.Application.Queries.Admin;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.Admin;

internal sealed class GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLog>>
{
    public async Task<IReadOnlyList<AuditLog>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        => await auditLogRepository.GetFilteredAsync(request.Email, request.From, request.To, request.Action);
}
