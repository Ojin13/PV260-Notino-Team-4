using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Infrastructure.Data.Seeders;

namespace Popocatepetl.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var dbContext =  serviceProvider.GetRequiredService<PopocatepetlDbContext>();
        await ReportSeeder.SeedAsync(dbContext);
        await DiffDataSeeder.SeedAsync(dbContext);
        await DiffResultSeeder.SeedAsync(dbContext);
        await AuditLogSeeder.SeedAsync(dbContext);
    }
    
}