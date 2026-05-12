namespace Popocatepetl.CLI.Prompts;

public interface ITextPrompt
{
    Task<string> AskAsync(
        string labelKey,
        Func<string, ValidationResult>? validate = null,
        bool secret = false,
        CancellationToken ct = default);
}

public readonly record struct ValidationResult(bool IsValid, string? ErrorKey, object[]? ErrorArgs)
{
    public static ValidationResult Ok() => new(true, null, null);

    public static ValidationResult Error(string errorKey, params object[] args) =>
        new(false, errorKey, args.Length == 0 ? null : args);
}
