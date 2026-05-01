namespace Popocatepetl.CLI.Prompts;

public interface ISelectPrompt
{
    Task<T> AskAsync<T>(
        string titleKey,
        IReadOnlyList<SelectChoice<T>> choices,
        CancellationToken ct = default) where T : notnull;
}

public sealed record SelectChoice<T>(T Value, string LabelKey, bool IsCurrent = false) where T : notnull;