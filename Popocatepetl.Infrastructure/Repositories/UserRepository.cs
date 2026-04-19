using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

/// <summary>EF Core implementation of IUserRepository.</summary>
public sealed class UserRepository(PopocatepetlDbContext context) : IUserRepository
{
    public async Task<IEnumerable<AppUser>> GetAllAsync() =>
        await context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

    public async Task<AppUser?> GetByIdAsync(Guid id) =>
        await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(AppUser user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppUser user)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await context.Users
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync();
    }
}
