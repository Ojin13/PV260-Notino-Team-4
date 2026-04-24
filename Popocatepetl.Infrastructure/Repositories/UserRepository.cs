using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

public sealed class UserRepository(PopocatepetlDbContext context) : Repository<AppUser>(context), IUserRepository
{
    public override async Task<IEnumerable<AppUser>> GetAllAsync() =>
        await Context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
}
