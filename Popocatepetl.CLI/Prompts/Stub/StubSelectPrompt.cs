using Microsoft.Extensions.Localization;
using Popocatepetl.CLI.Localization;

namespace Popocatepetl.CLI.Prompts.Stub;

public sealed class StubSelectPrompt : ISelectPrompt
{
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly LocaleState _locale;

    public StubSelectPrompt(IStringLocalizer<CliStrings> loc, LocaleState locale)
    {
        _loc = loc;
        _locale = locale;
    }

    public Task<T> AskAsync<T>(
        string titleKey,
        IReadOnlyList<SelectChoice<T>> choices,
        CancellationToken ct = default) where T : notnull
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            _locale.AlignCurrentThread();
            Console.WriteLine();
            Console.WriteLine($"== {_loc[titleKey].Value} ==");
            for (var i = 0; i < choices.Count; i++)
            {
                var marker = choices[i].IsCurrent ? "*" : " ";
                Console.WriteLine($"  [{i + 1}] {marker} {_loc[choices[i].LabelKey].Value}");
            }
            Console.Write("Choose [1-" + choices.Count + "]: ");

            var input = Console.ReadLine();
            if (int.TryParse(input, out var idx) && idx >= 1 && idx <= choices.Count)
            {
                return Task.FromResult(choices[idx - 1].Value);
            }
            Console.WriteLine("  ! invalid choice");
        }
    }
}
