using System.Net.Mail;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Services;

namespace Popocatepetl.CLI.Menus;

public sealed class EmailLoginPrompt
{
    private readonly ITextPrompt _text;
    private readonly CurrentUserContext _session;

    public EmailLoginPrompt(ITextPrompt text, CurrentUserContext session)
    {
        _text = text;
        _session = session;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var email = await _text.AskAsync(
            "auth.email.prompt",
            validate: ValidateEmail,
            ct: ct);

        _session.Email = email;
    }

    private static ValidationResult ValidateEmail(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ValidationResult.Error("auth.email.empty");
        }
        return MailAddress.TryCreate(input, out _)
            ? ValidationResult.Ok()
            : ValidationResult.Error("auth.email.invalid");
    }
}
