using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Persistence contract for AuditLog entries.</summary>
public interface IAuditLogRepository
{
    /// <summary>Appends a new audit log entry.</summary>
    Task AddAsync(AuditLog log);

    /// <summary>Returns all audit log entries.</summary>
    Task<IReadOnlyList<AuditLog>> GetAllAsync();

    /// <summary>Returns all audit log entries recorded for the given user email.</summary>
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(string email);
}
