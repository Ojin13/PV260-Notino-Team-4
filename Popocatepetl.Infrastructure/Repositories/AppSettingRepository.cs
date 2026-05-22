using Microsoft.EntityFrameworkCore;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Infrastructure.Repositories;

public sealed class AppSettingRepository(PopocatepetlDbContext context) : IAppSettingRepository
{
    public async Task<string?> GetAsync(AppSettingType type)
    {
        var existing = await context.Set<AppSetting>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Type == type);
        return existing?.Value;
    }

    public async Task SetAsync(AppSettingType type, string value)
    {
        var existing = await context.Set<AppSetting>().FirstOrDefaultAsync(s => s.Type == type);
        if (existing is null)
        {
            await context.Set<AppSetting>().AddAsync(AppSetting.Create(type, value));
        }
        else
        {
            existing.UpdateValue(value);
        }
        await context.SaveChangesAsync();
    }
}
