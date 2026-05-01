using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Enums;
using Spectre.Console;

namespace Popocatepetl.CLI.Navigation;

public sealed class MenuChrome
{
    private readonly IAnsiConsole _console;
    private readonly ICurrentUserContext _session;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;

    public MenuChrome(
        IAnsiConsole console,
        ICurrentUserContext session,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme)
    {
        _console = console;
        _session = session;
        _loc = loc;
        _theme = theme;
    }

    public void RenderTop()
    {
        var palette = _theme.Active;

        var email = string.IsNullOrEmpty(_session.Email) ? "—" : _session.Email;
        var roleLabel = _session.Role == UserRole.None
            ? null
            : _loc[$"menu.role.{_session.Role.ToString().ToLowerInvariant()}"].Value;

        var context = roleLabel is null
            ? Markup.Escape(email)
            : $"{Markup.Escape(email)}  ·  {Markup.Escape(roleLabel)}";

        var rule = new Rule(
            $"[{palette.Heading}]Popocatepetl[/]  [{palette.Muted}]{context}[/]")
            .RuleStyle(palette.BorderStyle())
            .LeftJustified();

        _console.Write(rule);
    }

    public void WaitForContinue()
    {
        if (Console.IsInputRedirected)
        {
            return;
        }
        var palette = _theme.Active;
        _console.WriteLine();
        _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["prompt.continue"].Value)}[/]");
        _console.Input.ReadKey(intercept: true);
    }
}
