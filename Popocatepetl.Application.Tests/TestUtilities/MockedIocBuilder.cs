using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.TestUtilities;

/// <summary>Fluent builder for an IoC container used in handler tests.</summary>
public class MockedIocBuilder
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    public MockedIocBuilder()
    {
        _serviceCollection.AddLogging();
    }

    public MockedIocBuilder AddMediatR(Assembly handlerAssembly)
    {
        _serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(handlerAssembly));
        return this;
    }

    public MockedIocBuilder AddMockedEmailService(out Mock<IEmailService> mock)
    {
        mock = new Mock<IEmailService>();
        _serviceCollection.AddSingleton(mock.Object);
        return this;
    }

    public MockedIocBuilder AddSingleton<T>(T instance)
        where T : class
    {
        _serviceCollection.AddSingleton(instance);
        return this;
    }

    public ServiceProvider Build() => _serviceCollection.BuildServiceProvider();
}