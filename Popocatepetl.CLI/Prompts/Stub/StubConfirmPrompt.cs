using Microsoft.Extensions.Localization;
using Popocatepetl.CLI.Localization;

namespace Popocatepetl.CLI.Prompts.Stub;

public sealed class StubConfirmPrompt : IConfirmPrompt
{
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly LocaleState _locale;

    public StubConfirmPrompt(IStringLocalizer<CliStrings> loc, LocaleState locale)
    {
        _loc = loc;
        _locale = locale;
    }

    public Task<bool> AskAsync(string labelKey, bool defaultValue = false, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _locale.AlignCurrentThread();
        var hint = defaultValue ? "[Y/n]" : "[y/N]";
        Console.Write($"{_loc[labelKey].Value} {hint}: ");
        var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();
        var answer = input switch
        {
            "y" or "yes" => true,
            "n" or "no" => false,
            "" => defaultValue,
            _ => defaultValue,
        };
        return Task.FromResult(answer);
    }
}
