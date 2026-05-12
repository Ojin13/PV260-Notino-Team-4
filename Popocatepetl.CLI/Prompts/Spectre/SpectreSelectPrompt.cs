using Microsoft.Extensions.Localization;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Theming;
using Spectre.Console;

namespace Popocatepetl.CLI.Prompts.Spectre;

public sealed class SpectreSelectPrompt : ISelectPrompt
{
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly LocaleState _locale;
    private readonly ThemeApplier _theme;

    public SpectreSelectPrompt(
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

    public async Task<T> AskAsync<T>(
        string titleKey,
        IReadOnlyList<SelectChoice<T>> choices,
        CancellationToken ct = default) where T : notnull
    {
        _locale.AlignCurrentThread();
        var palette = _theme.Active;

        var prompt = new SelectionPrompt<SelectChoice<T>>()
            .Title($"[{palette.Heading}]{Markup.Escape(_loc[titleKey].Value)}[/]")
            .HighlightStyle(palette.HighlightStyle())
            .UseConverter(c =>
            {
                var label = Markup.Escape(_loc[c.LabelKey].Value);
                return c.IsCurrent
                    ? $"[{palette.Success}]●[/] {label}"
                    : $"  {label}";
            })
            .AddChoices(choices);

        var picked = await prompt.ShowAsync(_console, ct);
        return picked.Value;
    }
}
