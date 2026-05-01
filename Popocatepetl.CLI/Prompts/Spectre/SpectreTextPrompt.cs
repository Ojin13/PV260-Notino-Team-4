using Microsoft.Extensions.Localization;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Theming;
using Spectre.Console;
using SpectreValidationResult = Spectre.Console.ValidationResult;
using PromptValidationResult = Popocatepetl.CLI.Prompts.ValidationResult;

namespace Popocatepetl.CLI.Prompts.Spectre;

public sealed class SpectreTextPrompt : ITextPrompt
{
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly LocaleState _locale;
    private readonly ThemeApplier _theme;

    public SpectreTextPrompt(
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

    public async Task<string> AskAsync(
        string labelKey,
        Func<string, PromptValidationResult>? validate = null,
        bool secret = false,
        CancellationToken ct = default)
    {
        _locale.AlignCurrentThread();
        var palette = _theme.Active;

        var prompt = new TextPrompt<string>(_loc[labelKey].Value + ":")
            .PromptStyle(palette.HighlightStyle())
            .AllowEmpty();

        if (secret)
        {
            prompt = prompt.Secret('*');
        }

        if (validate is not null)
        {
            prompt = prompt.Validate(input =>
            {
                var result = validate(input);
                if (result.IsValid)
                {
                    return SpectreValidationResult.Success();
                }
                var key = result.ErrorKey ?? "validation.error";
                var msg = result.ErrorArgs is { Length: > 0 } args
                    ? _loc[key, args].Value
                    : _loc[key].Value;
                return SpectreValidationResult.Error(msg);
            });
        }

        return await prompt.ShowAsync(_console, ct);
    }
}
