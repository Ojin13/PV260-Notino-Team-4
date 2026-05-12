using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Enums;
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
    protected Mock<IAuditLogRepository> AuditLogRepository { get; }
    protected Mock<ICurrentUserContext> CurrentUserContext { get; }

    protected HandlerTestBase()
    {
        var builder = new MockedIocBuilder();
        builder
            .AddApplication()
            .AddMockedEmailService(out var emailService)
            .AddMockedDiffResultRepository(out var diffResultRepository)
            .AddMockedDiffExporter(out var diffExporter)
            .AddMockedAuditLogRepository(out var auditLogRepository)
            .AddMockedCurrentUserContext(out var currentUserContext);

        _serviceProvider = builder.Build();
        Mediator = _serviceProvider.GetRequiredService<IMediator>();
        EmailService = emailService;
        DiffResultRepository = diffResultRepository;
        DiffExporter = diffExporter;
        AuditLogRepository = auditLogRepository;
        CurrentUserContext = currentUserContext;
        CurrentUserContext.SetupGet(x => x.Email).Returns("tester@example.com");
        CurrentUserContext.SetupGet(x => x.Role).Returns(UserRole.Admin);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
