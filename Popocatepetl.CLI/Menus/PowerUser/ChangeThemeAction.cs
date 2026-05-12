using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Spectre.Console;

namespace Popocatepetl.CLI.Menus.PowerUser;

public sealed class ChangeThemeAction : IMenuAction
{
    private readonly ISelectPrompt _select;
    private readonly ThemeStore _store;
    private readonly ThemeApplier _applier;
    private readonly IAuditLogRepository _audit;
    private readonly ICurrentUserContext _session;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;

    public ChangeThemeAction(
        ISelectPrompt select,
        ThemeStore store,
        ThemeApplier applier,
        IAuditLogRepository audit,
        ICurrentUserContext session,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc)
    {
        _select = select;
        _store = store;
        _applier = applier;
        _audit = audit;
        _session = session;
        _console = console;
        _loc = loc;
    }

    public string LabelKey => "menu.poweruser.theme.change";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var current = _applier.Active;
        WriteSwatchTable(current);

        var choices = BuiltInPalettes.All
            .Select(p => new SelectChoice<ThemePalette>(p, p.DisplayKey, IsCurrent: p.Id == current.Id))
            .ToList();

        var picked = await _select.AskAsync("menu.poweruser.theme.title", choices, ct);
        if (picked.Id == current.Id)
        {
            return;
        }

        await _store.SetActiveAsync(picked.Id, ct);
        _applier.Apply(picked);

        await _audit.AddAsync(AuditLog.Create(
            _session.Email,
            "ChangeTheme",
            DateTime.UtcNow,
            wasSuccessful: true,
            details: $"Palette={picked.Id}"));
    }

    private void WriteSwatchTable(ThemePalette current)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(current.BorderStyle())
            .AddColumn(new TableColumn(new Markup($"[{current.Heading}]{Markup.Escape(_loc["theme.col.palette"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{current.Heading}]{Markup.Escape(_loc["theme.col.heading"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{current.Heading}]{Markup.Escape(_loc["theme.col.highlight"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{current.Heading}]{Markup.Escape(_loc["theme.col.success"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{current.Heading}]{Markup.Escape(_loc["theme.col.warning"].Value)}[/]")))
            .AddColumn(new TableColumn(new Markup($"[{current.Heading}]{Markup.Escape(_loc["theme.col.error"].Value)}[/]")));

        foreach (var palette in BuiltInPalettes.All)
        {
            var label = palette.Id == current.Id
                ? $"[{current.Highlight}]{Markup.Escape(_loc[palette.DisplayKey].Value)} ★[/]"
                : Markup.Escape(_loc[palette.DisplayKey].Value);

            table.AddRow(
                new Markup(label),
                Swatch(palette.Heading),
                Swatch(palette.Highlight),
                Swatch(palette.Success),
                Swatch(palette.Warning),
                Swatch(palette.Error));
        }

        _console.Write(table);
    }

    private static Markup Swatch(string color) => new($"[{color}]██████[/] {color}");
}
