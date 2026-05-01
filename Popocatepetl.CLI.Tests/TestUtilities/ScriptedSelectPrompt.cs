using Popocatepetl.CLI.Prompts;

namespace Popocatepetl.CLI.Tests.TestUtilities;

internal sealed class ScriptedSelectPrompt : ISelectPrompt
{
    private readonly Queue<string> _picks;

    public List<string> AskedTitleKeys { get; } = new();
    public List<IReadOnlyList<string>> OfferedChoiceLabels { get; } = new();

    public ScriptedSelectPrompt(params string[] picks)
    {
        _picks = new Queue<string>(picks);
    }

    public Task<T> AskAsync<T>(
        string titleKey,
        IReadOnlyList<SelectChoice<T>> choices,
        CancellationToken ct = default) where T : notnull
    {
        AskedTitleKeys.Add(titleKey);
        OfferedChoiceLabels.Add(choices.Select(c => c.LabelKey).ToList());

        if (_picks.Count == 0)
        {
            throw new InvalidOperationException($"No scripted pick available for prompt '{titleKey}'.");
        }

        var labelToPick = _picks.Dequeue();
        var match = choices.FirstOrDefault(c => c.LabelKey == labelToPick)
            ?? throw new InvalidOperationException(
                $"Scripted pick '{labelToPick}' not present in prompt '{titleKey}'. " +
                $"Available: {string.Join(", ", choices.Select(c => c.LabelKey))}");

        return Task.FromResult(match.Value);
    }
}
