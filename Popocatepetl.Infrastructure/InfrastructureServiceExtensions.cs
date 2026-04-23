using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;
using Popocatepetl.Infrastructure.Email;
using Popocatepetl.Infrastructure.Repositories;
using Resend;

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

        services.AddOptions<ResendOptions>()
            .Bind(configuration.GetSection(ResendOptions.SectionName));
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration[$"{ResendOptions.SectionName}:ApiKey"] ?? string.Empty;
        });
        services.AddTransient<IResend, ResendClient>();
        services.AddScoped<IEmailService, ResendEmailService>();

        return services;
    }
}
