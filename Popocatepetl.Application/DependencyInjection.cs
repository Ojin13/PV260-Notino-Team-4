using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Behaviors;

namespace Popocatepetl.Application;

/// <summary>Registers all Application-layer services into the DI container.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuditLoggingBehavior<,>));
        });

        return services;
    }
}
