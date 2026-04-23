using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

public class Repository<T>(PopocatepetlDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly PopocatepetlDbContext Context = context;

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await context.Set<T>().AsNoTracking().ToListAsync();

    public async Task<T?> GetByIdAsync(Guid id) =>
        await context.Set<T>().FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        context.Set<T>().Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await context.Set<T>().Where(e => e.Id == id).ExecuteDeleteAsync();
    }
}
