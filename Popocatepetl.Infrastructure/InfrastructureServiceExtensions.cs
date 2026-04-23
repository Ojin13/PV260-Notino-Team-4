using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;
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
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDiffResultRepository, DiffResultRepository>();

        return services;
    }
}
