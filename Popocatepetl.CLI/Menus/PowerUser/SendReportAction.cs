using System.Net.Mail;
using MediatR;
using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Theming;
using Spectre.Console;
using ValidationResult = Popocatepetl.CLI.Prompts.ValidationResult;

namespace Popocatepetl.CLI.Menus.PowerUser;

public sealed class SendReportAction : IMenuAction
{
    private const int MaxRecipients = 100;

    private readonly ISender _sender;
    private readonly ITextPrompt _text;
    private readonly ISelectPrompt _select;
    private readonly IConfirmPrompt _confirm;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;
    private readonly MenuChrome _chrome;

    public SendReportAction(
        ISender sender,
        ITextPrompt text,
        ISelectPrompt select,
        IConfirmPrompt confirm,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme,
        MenuChrome chrome)
    {
        _sender = sender;
        _text = text;
        _select = select;
        _confirm = confirm;
        _console = console;
        _loc = loc;
        _theme = theme;
        _chrome = chrome;
    }

    public string LabelKey => "menu.poweruser.send";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var palette = _theme.Active;

        var raw = await _text.AskAsync(
            "menu.poweruser.send.recipients",
            validate: ValidateList,
            ct: ct);

        if (string.IsNullOrWhiteSpace(raw))
        {
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["status.cancelled"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var recipients = ParseRecipients(raw);

        var format = await _select.AskAsync(
            "menu.poweruser.send.format",
            new[]
            {
                new SelectChoice<DiffExportFormat>(DiffExportFormat.Pdf, "format.pdf"),
                new SelectChoice<DiffExportFormat>(DiffExportFormat.Csv, "format.csv"),
            },
            ct);

        _console.MarkupLine(
            $"[{palette.Heading}]{Markup.Escape(_loc["send.recipients.summary", recipients.Count, format].Value)}[/]");
        foreach (var r in recipients)
        {
            _console.MarkupLine($"  [{palette.Highlight}]•[/] {Markup.Escape(r)}");
        }

        if (!await _confirm.AskAsync("menu.poweruser.send.confirm", defaultValue: true, ct))
        {
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["status.cancelled"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        try
        {
            await _console
                .Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_loc["status.sending"].Value, async _ =>
                    await _sender.Send(new SendEmailCommand(recipients, format), ct));

            _console.MarkupLine($"[{palette.Success}]✓ {Markup.Escape(_loc["status.sent"].Value)}[/]");
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[{palette.Error}]{Markup.Escape(_loc["error.send", ex.Message].Value)}[/]");
        }
        _chrome.WaitForContinue();
    }

    internal static IReadOnlyList<string> ParseRecipients(string raw) =>
        raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    internal static ValidationResult ValidateList(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return ValidationResult.Ok();
        }
        var parts = ParseRecipients(raw);
        if (parts.Count > MaxRecipients) return ValidationResult.Error("menu.poweruser.send.too-many");
        foreach (var p in parts)
        {
            if (!MailAddress.TryCreate(p, out _))
            {
                return ValidationResult.Error("menu.poweruser.send.invalid", p);
            }
        }
        return ValidationResult.Ok();
    }
}
