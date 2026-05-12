using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries.Admin;

public record GetAuditLogsQuery(
    string? Email,
    DateTime? From,
    DateTime? To,
    string? Action) : IAuditableRequest, IRequest<IReadOnlyList<AuditLog>>
{
    public string ActionName => "ViewAuditLogs";

    public string Detail => "";
}