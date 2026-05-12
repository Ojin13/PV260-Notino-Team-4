using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IReadOnlyList<AuditLog>> GetAllAsync();
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(string email);
    Task<IReadOnlyList<AuditLog>> GetFilteredAsync(string? email, DateTime? from, DateTime? to, string? action);
}
