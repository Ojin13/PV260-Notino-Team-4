using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;
using Popocatepetl.Infrastructure.Data.Seeders;
using Popocatepetl.Infrastructure.Repositories;

namespace Popocatepetl.Infrastructure;

/// <summary>Registers all Infrastructure-layer services into the DI container.</summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbPath = configuration["Database:Path"]
            ?? throw new InvalidOperationException("Missing configuration key 'Database:Path'.");

        services.AddDbContext<PopocatepetlDbContext>(options =>
            options
                .UseSqlite($"Data Source={dbPath}")
                .UseSeeding((context, _) =>
                {
                    var db = (PopocatepetlDbContext)context;
                    ReportSeeder.SeedAsync(db).GetAwaiter().GetResult();
                    DiffDataSeeder.SeedAsync(db).GetAwaiter().GetResult();
                    DiffResultSeeder.SeedAsync(db).GetAwaiter().GetResult();
                    AuditLogSeeder.SeedAsync(db).GetAwaiter().GetResult();
                })
                .UseAsyncSeeding(async (context, _, ct) =>
                {
                    var db = (PopocatepetlDbContext)context;
                    await ReportSeeder.SeedAsync(db);
                    await DiffDataSeeder.SeedAsync(db);
                    await DiffResultSeeder.SeedAsync(db);
                    await AuditLogSeeder.SeedAsync(db);
                }));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IDiffResultRepository, DiffResultRepository>();
        services.AddScoped<IDiffCalculator, DiffCalculator>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}
