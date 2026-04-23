using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Tests.TestUtilities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Tests.Handlers.SendEmail;

public class SendEmailCommandHandlerTests : HandlerTestBase
{
    private const string Subject = "Hello";
    private const string Body = "World";
    private static readonly IReadOnlyList<string> Recipients = ["alice@example.com", "bob@example.com"];

    [Fact]
    public async Task Handle_WithValidInput_CallsEmailServiceOnce()
    {
        var command = new SendEmailCommand(Subject, Body, Recipients);

        await Mediator.Send(command);

        EmailService.Verify(
            s => s.SendAsync(Subject, Body, Recipients),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRecipients_ThrowsAndDoesNotCallService()
    {
        var command = new SendEmailCommand(Subject, Body, []);

        var act = async () => await Mediator.Send(command);

        await act.Should().ThrowAsync<ArgumentException>();
        EmailService.Verify(
            s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithMissingSubject_Throws(string subject)
    {
        var command = new SendEmailCommand(subject, Body, Recipients);

        var act = async () => await Mediator.Send(command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithMissingBody_Throws(string body)
    {
        var command = new SendEmailCommand(Subject, body, Recipients);

        var act = async () => await Mediator.Send(command);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithMalformedRecipient_Throws()
    {
        var command = new SendEmailCommand(Subject, Body, ["alice@example.com", "not-an-email"]);

        var act = async () => await Mediator.Send(command);

        (await act.Should().ThrowAsync<ArgumentException>())
            .Which.Message.Should().Contain("not-an-email");
        EmailService.Verify(
            s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrows_PropagatesException()
    {
        EmailService
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new InvalidOperationException("resend down"));

        var command = new SendEmailCommand(Subject, Body, Recipients);

        var act = async () => await Mediator.Send(command);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("resend down");
    }
}