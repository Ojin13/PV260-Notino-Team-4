using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Theming;
using Spectre.Console;

namespace Popocatepetl.CLI.Auth;

public sealed class AdminPasswordGate
{
    private readonly AdminOptions _options;
    private readonly ITextPrompt _text;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;

    public AdminPasswordGate(
        IOptions<AdminOptions> options,
        ITextPrompt text,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme)
    {
        _options = options.Value;
        _text = text;
        _console = console;
        _loc = loc;
        _theme = theme;
    }

    public async Task<bool> TryUnlockAsync(CancellationToken ct)
    {
        var entered = await _text.AskAsync(
            "auth.admin.password.prompt",
            secret: true,
            ct: ct);

        if (entered == _options.Password)
        {
            return true;
        }

        _console.MarkupLine(
            $"[{_theme.Active.Error}]{Markup.Escape(_loc["auth.admin.password.invalid"].Value)}[/]");
        return false;
    }
}
