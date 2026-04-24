using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.TestUtilities;

/// <summary>Base class for handler tests — spins up a real MediatR pipeline with mocked dependencies.</summary>
public abstract class HandlerTestBase : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    protected IMediator Mediator { get; }
    protected Mock<IEmailService> EmailService { get; }
    protected Mock<IDiffResultRepository> DiffResultRepository { get; }
    protected Mock<IDiffExporter> DiffExporter { get; }

    protected HandlerTestBase()
    {
        var builder = new MockedIocBuilder();
        builder
            .AddMockedEmailService(out var emailService)
            .AddMockedDiffResultRepository(out var diffResultRepository)
            .AddMockedDiffExporter(out var diffExporter)
            .AddMediatR(typeof(DependencyInjection).Assembly);

        _serviceProvider = builder.Build();
        Mediator = _serviceProvider.GetRequiredService<IMediator>();
        EmailService = emailService;
        DiffResultRepository = diffResultRepository;
        DiffExporter = diffExporter;
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}