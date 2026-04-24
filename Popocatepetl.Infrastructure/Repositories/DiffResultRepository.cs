using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

public sealed class DiffResultRepository(PopocatepetlDbContext context) : Repository<DiffResult>(context), IDiffResultRepository
{
    public async Task<DiffResult?> GetLastAsync() =>
        await Context.DiffResults
            .Include(x => x.DiffDataEntries)
            .AsNoTracking()
            .OrderByDescending(d => d.GeneratedAt)
            .FirstOrDefaultAsync();
}
