using Popocatepetl.CLI.Auth;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Tests.TestUtilities;
using Popocatepetl.CLI.Theming;
using Spectre.Console.Testing;

namespace Popocatepetl.CLI.Tests.Auth;

public class AdminPasswordGateTests
{
    private const string CorrectPassword = "JAHODOVÝDORT375";

    private static AdminPasswordGate BuildGate(Mock<ITextPrompt> prompt) => new(
        prompt.Object,
        new TestConsole(),
        new FakeStringLocalizer<CliStrings>(),
        new ThemeApplier());

    [Fact]
    public async Task TryUnlockAsync_CorrectPassword_ReturnsTrue()
    {
        var prompt = new Mock<ITextPrompt>();
        prompt.Setup(p => p.AskAsync(
                It.IsAny<string>(),
                It.IsAny<Func<string, ValidationResult>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CorrectPassword);

        var result = await BuildGate(prompt).TryUnlockAsync(default);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryUnlockAsync_WrongPassword_ReturnsFalse()
    {
        var prompt = new Mock<ITextPrompt>();
        prompt.Setup(p => p.AskAsync(
                It.IsAny<string>(),
                It.IsAny<Func<string, ValidationResult>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("not-the-password");

        var result = await BuildGate(prompt).TryUnlockAsync(default);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryUnlockAsync_PromptsForSecretInput()
    {
        var prompt = new Mock<ITextPrompt>();
        prompt.Setup(p => p.AskAsync(
                It.IsAny<string>(),
                It.IsAny<Func<string, ValidationResult>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CorrectPassword);

        await BuildGate(prompt).TryUnlockAsync(default);

        prompt.Verify(
            p => p.AskAsync(
                "auth.admin.password.prompt",
                It.IsAny<Func<string, ValidationResult>?>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
