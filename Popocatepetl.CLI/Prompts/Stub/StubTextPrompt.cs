using Microsoft.Extensions.Localization;
using Popocatepetl.CLI.Localization;

namespace Popocatepetl.CLI.Prompts.Stub;

public sealed class StubTextPrompt : ITextPrompt
{
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly LocaleState _locale;

    public StubTextPrompt(IStringLocalizer<CliStrings> loc, LocaleState locale)
    {
        _loc = loc;
        _locale = locale;
    }

    public Task<string> AskAsync(
        string labelKey,
        Func<string, ValidationResult>? validate = null,
        bool secret = false,
        CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            _locale.AlignCurrentThread();
            Console.Write($"{_loc[labelKey].Value}: ");
            var input = secret && !Console.IsInputRedirected
                ? ReadSecret()
                : (Console.ReadLine() ?? string.Empty);

            if (validate is null)
            {
                return Task.FromResult(input);
            }

            var result = validate(input);
            if (result.IsValid)
            {
                return Task.FromResult(input);
            }

            var key = result.ErrorKey ?? "validation.error";
            var msg = result.ErrorArgs is { Length: > 0 } args
                ? _loc[key, args].Value
                : _loc[key].Value;
            Console.WriteLine($"  ! {msg}");
        }
    }

    private static string ReadSecret()
    {
        var buffer = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return buffer.ToString();
            }
            if (key.Key == ConsoleKey.Backspace && buffer.Length > 0)
            {
                buffer.Length--;
                continue;
            }
            if (!char.IsControl(key.KeyChar))
            {
                buffer.Append(key.KeyChar);
            }
        }
    }
}
