namespace Popocatepetl.CLI.Prompts;

public interface IConfirmPrompt
{
    Task<bool> AskAsync(string labelKey, bool defaultValue = false, CancellationToken ct = default);
}