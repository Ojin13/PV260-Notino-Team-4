using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Common;
using Popocatepetl.Application;
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

    public MockedIocBuilder AddApplication()
    {
        _serviceCollection.AddApplication();
        return this;
    }

    public MockedIocBuilder AddMediatR(Assembly handlerAssembly)
    {
        _serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(handlerAssembly));
        return this;
    }

    public MockedIocBuilder AddMocked<T>(out Mock<T> mock)
        where T : class
    {
        mock = new Mock<T>();
        _serviceCollection.AddSingleton(mock.Object);
        return this;
    }

    public MockedIocBuilder AddMockedEmailService(out Mock<IEmailService> mock)
    {
        return AddMocked(out mock);
    }

    public MockedIocBuilder AddMockedDiffResultRepository(out Mock<IDiffResultRepository> mock)
    {
        return AddMocked(out mock);
    }

    public MockedIocBuilder AddMockedDiffExporter(out Mock<IDiffExporter> mock)
    {
        return AddMocked(out mock);
    }

    public MockedIocBuilder AddMockedAuditLogRepository(out Mock<IAuditLogRepository> mock)
    {
        return AddMocked(out mock);
    }

    public MockedIocBuilder AddMockedCurrentUserContext(out Mock<ICurrentUserContext> mock)
    {
        return AddMocked(out mock);
    }

    public MockedIocBuilder AddSingleton<T>(T instance)
        where T : class
    {
        _serviceCollection.AddSingleton(instance);
        return this;
    }

    public ServiceProvider Build() => _serviceCollection.BuildServiceProvider();
}
