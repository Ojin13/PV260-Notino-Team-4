namespace Popocatepetl.CLI.Prompts;

public interface IFileSaveDialog
{
    bool IsAvailable { get; }

    Task<string?> ShowAsync(string title, string defaultName, CancellationToken ct = default);
}
