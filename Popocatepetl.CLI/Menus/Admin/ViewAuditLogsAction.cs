using MediatR;
using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Queries.Admin;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Theming;
using Spectre.Console;

namespace Popocatepetl.CLI.Menus.Admin;

public sealed class ViewAuditLogsAction : IMenuAction
{
    private const int RenderLimit = 25;

    private readonly ISender _sender;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;
    private readonly MenuChrome _chrome;
    private readonly LocaleState _locale;

    public ViewAuditLogsAction(
        ISender sender,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme,
        MenuChrome chrome,
        LocaleState locale)
    {
        _sender = sender;
        _console = console;
        _loc = loc;
        _theme = theme;
        _chrome = chrome;
        _locale = locale;
    }

    public string LabelKey => "menu.admin.auditlogs";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var logs = await _sender.Send(new GetAuditLogsQuery(null, null, null, null), ct);
        _locale.AlignCurrentThread();
        if (logs.Count == 0)
        {
            _console.MarkupLine($"[{_theme.Active.Muted}]{Markup.Escape(_loc["audit.empty"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var palette = _theme.Active;
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(palette.BorderStyle())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["audit.col.timestamp"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["audit.col.email"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["audit.col.action"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["audit.col.status"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["audit.col.details"].Value)}[/]")));

        foreach (var log in logs.Take(RenderLimit))
        {
            var statusMarkup = log.WasSuccessful
                ? $"[{palette.Success}]{Markup.Escape(_loc["audit.status.ok"].Value)}[/]"
                : $"[{palette.Error}]{Markup.Escape(_loc["audit.status.err"].Value)}[/]";

            table.AddRow(
                new Markup(Markup.Escape(log.OccurredAt.ToString("u"))),
                new Markup(Markup.Escape(log.UserEmail)),
                new Markup(Markup.Escape(LocalizeAction(log.Action))),
                new Markup(statusMarkup),
                new Markup(Markup.Escape(LocalizeDetail(log.Details))));
        }

        _console.Write(table);
        if (logs.Count > RenderLimit)
        {
            _console.MarkupLine(
                $"[{palette.Muted}]{Markup.Escape(_loc["audit.truncated", logs.Count - RenderLimit].Value)}[/]");
        }
        _chrome.WaitForContinue();
    }

    private string LocalizeAction(string action)
    {
        var s = _loc[$"audit.action.{action}"];
        return s.ResourceNotFound ? action : s.Value;
    }

    private string LocalizeDetail(string? detail)
    {
        if (string.IsNullOrEmpty(detail)) return string.Empty;
        var idx = detail.IndexOf('=');
        if (idx <= 0) return detail;
        var key = detail[..idx];
        var s = _loc[$"audit.detail.{key}"];
        return s.ResourceNotFound ? detail : $"{s.Value}={detail[(idx + 1)..]}";
    }
}
