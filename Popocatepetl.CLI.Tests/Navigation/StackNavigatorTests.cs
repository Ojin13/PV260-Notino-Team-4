using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Services;
using Popocatepetl.CLI.Tests.TestUtilities;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Enums;
using Spectre.Console.Testing;

namespace Popocatepetl.CLI.Tests.Navigation;

public class StackNavigatorTests
{
    private static (StackNavigator nav, CurrentUserContext session) Build(ScriptedSelectPrompt prompt)
    {
        var session = new CurrentUserContext { Email = "test@x.com", Role = UserRole.None };
        var console = new TestConsole();
        var theme = new ThemeApplier();
        var loc = new FakeStringLocalizer<CliStrings>();
        var chrome = new MenuChrome(console, session, loc, theme);
        var nav = new StackNavigator(prompt, session, chrome, console);
        return (nav, session);
    }

    private sealed class CountingAction : Popocatepetl.CLI.Navigation.IMenuAction
    {
        public string LabelKey { get; }
        public int CallCount { get; private set; }

        public CountingAction(string labelKey) => LabelKey = labelKey;

        public Task ExecuteAsync(CancellationToken ct)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task RunAsync_PicksExitImmediately_ReturnsCleanly()
    {
        var prompt = new ScriptedSelectPrompt("menu.exit");
        var (nav, _) = Build(prompt);
        var root = MenuNode.Branch("menu.top.title", UserRole.None);

        await nav.RunAsync(root, default);

        prompt.AskedTitleKeys.Should().ContainSingle().Which.Should().Be("menu.top.title");
    }

    [Fact]
    public async Task RunAsync_PicksLeaf_RunsItsAction()
    {
        var leaf = new CountingAction("leaf.label");
        var prompt = new ScriptedSelectPrompt("leaf.label", "menu.exit");
        var (nav, _) = Build(prompt);
        var root = MenuNode.Branch("menu.top.title", UserRole.None,
            MenuNode.Leaf(leaf.LabelKey, UserRole.None, leaf));

        await nav.RunAsync(root, default);

        leaf.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_PicksBranch_RendersChildrenThenBack()
    {
        var leaf = new CountingAction("inner.leaf");
        var branch = MenuNode.Branch("inner.title", UserRole.None,
            MenuNode.Leaf(leaf.LabelKey, UserRole.None, leaf));
        var root = MenuNode.Branch("menu.top.title", UserRole.None, branch);
        var prompt = new ScriptedSelectPrompt("inner.title", "menu.back", "menu.exit");
        var (nav, _) = Build(prompt);

        await nav.RunAsync(root, default);

        prompt.AskedTitleKeys.Should().Equal("menu.top.title", "inner.title", "menu.top.title");
    }

    [Fact]
    public async Task RunAsync_RoleGatedChild_HiddenWhenSessionRoleTooLow()
    {
        var leaf = new CountingAction("admin.leaf");
        var root = MenuNode.Branch("menu.top.title", UserRole.None,
            MenuNode.Leaf(leaf.LabelKey, UserRole.Admin, leaf));
        var prompt = new ScriptedSelectPrompt("menu.exit");
        var (nav, session) = Build(prompt);
        session.Role = UserRole.User;

        await nav.RunAsync(root, default);

        prompt.OfferedChoiceLabels.Should().ContainSingle()
            .Which.Should().NotContain("admin.leaf").And.Contain("menu.exit");
        leaf.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_EntryGuardReturnsFalse_DoesNotPushBranch()
    {
        var inner = MenuNode.Branch("inner.title", UserRole.None);
        var root = MenuNode.Branch("menu.top.title", UserRole.None,
            MenuNode.GuardedBranch("guarded.label", UserRole.None,
                _ => Task.FromResult(false),
                inner));
        var prompt = new ScriptedSelectPrompt("guarded.label", "menu.exit");
        var (nav, _) = Build(prompt);

        await nav.RunAsync(root, default);

        prompt.AskedTitleKeys.Should().Equal("menu.top.title", "menu.top.title");
    }

    [Fact]
    public async Task RunAsync_EntryGuardReturnsTrue_PushesBranchAndCanRunSetSideEffect()
    {
        var leaf = new CountingAction("guarded.leaf");
        var root = MenuNode.Branch("menu.top.title", UserRole.None,
            MenuNode.GuardedBranch("guarded.label", UserRole.None,
                _ => Task.FromResult(true),
                MenuNode.Leaf(leaf.LabelKey, UserRole.None, leaf)));
        var prompt = new ScriptedSelectPrompt("guarded.label", "guarded.leaf", "menu.back", "menu.exit");
        var (nav, _) = Build(prompt);

        await nav.RunAsync(root, default);

        leaf.CallCount.Should().Be(1);
        prompt.AskedTitleKeys.Should().Equal(
            "menu.top.title", "guarded.label", "guarded.label", "menu.top.title");
    }
}
