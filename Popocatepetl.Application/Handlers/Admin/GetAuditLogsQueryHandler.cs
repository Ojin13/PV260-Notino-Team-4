using MediatR;
using Microsoft.Extensions.Logging;
using Popocatepetl.Application.Queries.Admin;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Handlers.Admin;

internal sealed class GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository, ILogger<GetAuditLogsQueryHandler> logger)
    : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLog>>
{
    public async Task<IReadOnlyList<AuditLog>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching audit logs for user: {Email}", request.Email);
        var auditLogs = await auditLogRepository.GetFilteredAsync(request.Email, request.From, request.To, request.Action);
        logger.LogInformation("Retrieved {Count} audit logs", auditLogs.Count);
        return auditLogs;
    }
}
