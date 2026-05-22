using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application;
using Popocatepetl.Infrastructure;

namespace Popocatepetl.Host;

public static class HostServiceExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }
}
