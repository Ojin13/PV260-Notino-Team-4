using MediatR;
using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Queries.UserRole;
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

    public ViewLatestDiffAction(
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

    public string LabelKey => "menu.user.diff.view";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var diff = await _sender.Send(new GetDiffReportQuery(), ct);
        var palette = _theme.Active;

        if (diff is null || diff.Count == 0)
        {
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["diff.empty"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(palette.BorderStyle())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Ticker[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Name[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Shares[/]")).RightAligned())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Δ %[/]")).RightAligned())
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Type[/]")))
            .AddColumn(new TableColumn(new Markup($"[{palette.Heading}]Weight %[/]")).RightAligned());

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
