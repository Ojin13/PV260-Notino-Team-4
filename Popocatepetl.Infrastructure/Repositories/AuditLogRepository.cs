using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

public sealed class AuditLogRepository(PopocatepetlDbContext context) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log)
    {
        await context.AuditLogs.AddAsync(log);
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync() =>
        await context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync();

    public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string email) =>
        await context.AuditLogs
            .AsNoTracking()
            .Where(x => x.UserEmail == email)
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync();

    public async Task<IReadOnlyList<AuditLog>> GetFilteredAsync(
        string? email, DateTime? from, DateTime? to, string? action)
    {
        var query = context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(email))
            query = query.Where(x => x.UserEmail == email);

        if (from.HasValue)
            query = query.Where(x => x.OccurredAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.OccurredAt <= to.Value);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(x => x.Action == action);

        return await query.OrderByDescending(x => x.OccurredAt).ToListAsync();
    }
}
