using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.Domain.Enums;
using Spectre.Console;

namespace Popocatepetl.CLI.Navigation;

public sealed class StackNavigator
{
    private const string BackKey = "menu.back";
    private const string ExitKey = "menu.exit";

    private readonly ISelectPrompt _select;
    private readonly ICurrentUserContext _session;
    private readonly MenuChrome _chrome;
    private readonly IAnsiConsole _console;
    private readonly LocaleState _locale;

    public StackNavigator(
        ISelectPrompt select,
        ICurrentUserContext session,
        MenuChrome chrome,
        IAnsiConsole console,
        LocaleState locale)
    {
        _select = select;
        _session = session;
        _chrome = chrome;
        _console = console;
        _locale = locale;
    }

    public async Task RunAsync(MenuNode root, CancellationToken ct)
    {
        var stack = new Stack<MenuNode>();
        stack.Push(root);

        while (stack.Count > 0 && !ct.IsCancellationRequested)
        {
            var current = stack.Peek();
            _console.Clear();
            if (stack.Count == 1)
            {
                _chrome.RenderTop();
            }
            var visible = current.Children.Where(IsVisible).ToList();

            var choices = new List<SelectChoice<MenuChoice>>(visible.Count + 1);
            foreach (var child in visible)
            {
                choices.Add(new SelectChoice<MenuChoice>(MenuChoice.ForNode(child), child.LabelKey));
            }
            choices.Add(stack.Count == 1
                ? new SelectChoice<MenuChoice>(MenuChoice.Exit, ExitKey)
                : new SelectChoice<MenuChoice>(MenuChoice.Back, BackKey));

            var picked = await _select.AskAsync(current.LabelKey, choices, ct);

            switch (picked.Kind)
            {
                case MenuChoiceKind.Exit:
                    return;
                case MenuChoiceKind.Back:
                    stack.Pop();
                    break;
                case MenuChoiceKind.Node when picked.Node!.IsLeaf:
                    _locale.AlignCurrentThread();
                    await picked.Node.Action!.ExecuteAsync(ct);
                    break;
                case MenuChoiceKind.Node:
                    if (picked.Node!.EntryGuard is null
                        || await picked.Node.EntryGuard(ct))
                    {
                        stack.Push(picked.Node);
                    }
                    else
                    {
                        _chrome.WaitForContinue();
                    }
                    break;
            }
        }
    }

    private bool IsVisible(MenuNode node) => node.RequiredRole <= _session.Role;

    private readonly record struct MenuChoice(MenuChoiceKind Kind, MenuNode? Node)
    {
        public static MenuChoice ForNode(MenuNode n) => new(MenuChoiceKind.Node, n);
        public static MenuChoice Back => new(MenuChoiceKind.Back, null);
        public static MenuChoice Exit => new(MenuChoiceKind.Exit, null);
    }

    private enum MenuChoiceKind { Node, Back, Exit }
}