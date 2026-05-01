using Microsoft.Extensions.Localization;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Theming;
using Spectre.Console;

namespace Popocatepetl.CLI.Prompts.Spectre;

public sealed class SpectreConfirmPrompt : IConfirmPrompt
{
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly LocaleState _locale;
    private readonly ThemeApplier _theme;

    public SpectreConfirmPrompt(
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        LocaleState locale,
        ThemeApplier theme)
    {
        _console = console;
        _loc = loc;
        _locale = locale;
        _theme = theme;
    }

    public async Task<bool> AskAsync(string labelKey, bool defaultValue = false, CancellationToken ct = default)
    {
        _locale.AlignCurrentThread();

        var prompt = new ConfirmationPrompt(_loc[labelKey].Value)
        {
            DefaultValue = defaultValue,
        };
        return await prompt.ShowAsync(_console, ct);
    }
}
