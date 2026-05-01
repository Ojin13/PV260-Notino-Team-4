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

    public ViewAuditLogsAction(
        ISender sender,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme,
        MenuChrome chrome)
    {
        _sender = sender;
        _console = console;
        _loc = loc;
        _theme = theme;
        _chrome = chrome;
    }

    public string LabelKey => "menu.admin.auditlogs";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var logs = await _sender.Send(new GetAuditLogsQuery(null, null, null, null), ct);
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
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Timestamp[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Email[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Action[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Status[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Details[/]")));

        foreach (var log in logs.Take(RenderLimit))
        {
            var statusMarkup = log.WasSuccessful
                ? $"[{palette.Success}]ok[/]"
                : $"[{palette.Error}]err[/]";

            table.AddRow(
                new Markup(Markup.Escape(log.OccurredAt.ToString("u"))),
                new Markup(Markup.Escape(log.UserEmail)),
                new Markup(Markup.Escape(log.Action)),
                new Markup(statusMarkup),
                new Markup(Markup.Escape(log.Details ?? string.Empty)));
        }

        _console.Write(table);
        if (logs.Count > RenderLimit)
        {
            _console.MarkupLine(
                $"[{palette.Muted}]({logs.Count - RenderLimit} older entries omitted)[/]");
        }
        _chrome.WaitForContinue();
    }
}
