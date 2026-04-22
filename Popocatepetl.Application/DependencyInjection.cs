using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Popocatepetl.Application;

/// <summary>Registers all Application-layer services into the DI container.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
