using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries.Admin;

public record GetAuditLogsQuery(
    string? Email,
    DateTime? From,
    DateTime? To,
    string? Action) : IRequest<IReadOnlyList<AuditLog>>;
