using MediatR;
using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Reports;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Enums;
using Spectre.Console;

namespace Popocatepetl.CLI.Menus.User;

public sealed class ViewLatestDiffAction : IMenuAction
{
    private readonly ISender _sender;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;
    private readonly MenuChrome _chrome;
    private readonly LocaleState _locale;

    public ViewLatestDiffAction(
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

    public string LabelKey => "menu.user.diff.view";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var diff = await _sender.Send(new GetDiffReportQuery(), ct);
        _locale.AlignCurrentThread();
        var palette = _theme.Active;

        if (diff is null)
        {
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["diff.empty"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        if (diff.Count == 0)
        {
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["diff.identical"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(palette.BorderStyle())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["diff.col.ticker"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["diff.col.name"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["diff.col.shares"].Value)}[/]")).RightAligned())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["diff.col.delta"].Value)}[/]")).RightAligned())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["diff.col.type"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]{Markup.Escape(_loc["diff.col.weight"].Value)}[/]")).RightAligned());

        foreach (var row in diff)
        {
            var typeColor = row.ShareDiffType switch
            {
                ShareDiffType.New => palette.Success,
                ShareDiffType.Increased => palette.Success,
                ShareDiffType.Decreased => palette.Error,
                _ => palette.Muted,
            };

            table.AddRow(
                new Markup($"[{palette.Highlight}]{Markup.Escape(row.Ticker)}[/]"),
                new Markup(Markup.Escape(row.Name)),
                new Markup(row.Shares.ToString("N0")),
                new Markup($"[{typeColor}]{row.SharesDiffPercent:+0.00;-0.00;0.00}[/]"),
                new Markup($"[{typeColor}]{row.ShareDiffType}[/]"),
                new Markup(row.WeightPercent.ToString("F2")));
        }

        _console.Write(table);
        _chrome.WaitForContinue();
    }
}
