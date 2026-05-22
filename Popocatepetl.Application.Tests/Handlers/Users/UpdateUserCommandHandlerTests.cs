using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Tests.TestUtilities;
using Popocatepetl.Application.Users;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Exceptions;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.Handlers.Users;

public sealed class UpdateUserCommandHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IAuditLogRepository> _auditLogRepository;
    private readonly Mock<ICurrentUserContext> _currentUserContext;

    public UpdateUserCommandHandlerTests()
    {
        var builder = new MockedIocBuilder();
        builder
            .AddApplication()
            .AddMocked(out _userRepository)
            .AddMocked(out _auditLogRepository)
            .AddMocked(out _currentUserContext);

        _serviceProvider = builder.Build();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _currentUserContext.SetupGet(x => x.Email).Returns("tester@example.com");
        _currentUserContext.SetupGet(x => x.Role).Returns(Popocatepetl.Domain.Enums.UserRole.Admin);
    }

    [Fact]
    public async Task Handle_WhenUserExists_UpdatesEmailAndPersistsEntity()
    {
        var user = AppUser.Create("old@example.com");
        _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

        await _mediator.Send(new UpdateUserCommand(user.Id, "new@example.com"));

        user.Email.Should().Be("new@example.com");
        _userRepository.Verify(r => r.UpdateAsync(It.Is<AppUser>(u =>
            u.Id == user.Id &&
            u.Email == "new@example.com")), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsNotFoundAndDoesNotPersist()
    {
        var id = Guid.NewGuid();
        _userRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((AppUser?)null);

        var act = async () => await _mediator.Send(new UpdateUserCommand(id, "new@example.com"));

        await act.Should().ThrowExactlyAsync<NotFoundException>();
        _userRepository.Verify(r => r.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
