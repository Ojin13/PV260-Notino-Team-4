using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

public sealed class DiffResultRepository(PopocatepetlDbContext context) : IDiffResultRepository
{
    public async Task<DiffResult?> GetLastAsync() =>
        await context.DiffResults
            .AsNoTracking()
            .OrderByDescending(d => d.GeneratedAt)
            .FirstOrDefaultAsync();

    public async Task CreateAsync(DiffResult diffResult)
    {
        await context.DiffResults.AddAsync(diffResult);
        await context.SaveChangesAsync();
    }
}