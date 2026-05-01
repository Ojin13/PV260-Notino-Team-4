using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Interfaces;
using Spectre.Console;

namespace Popocatepetl.CLI.Menus.Admin;

public sealed class ExportDiffAction : IMenuAction
{
    private readonly ISelectPrompt _select;
    private readonly ITextPrompt _text;
    private readonly IConfirmPrompt _confirm;
    private readonly IFileSaveDialog _dialog;
    private readonly IDiffResultRepository _diffRepo;
    private readonly IDiffExporter _exporter;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;
    private readonly MenuChrome _chrome;

    public ExportDiffAction(
        ISelectPrompt select,
        ITextPrompt text,
        IConfirmPrompt confirm,
        IFileSaveDialog dialog,
        IDiffResultRepository diffRepo,
        IDiffExporter exporter,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme,
        MenuChrome chrome)
    {
        _select = select;
        _text = text;
        _confirm = confirm;
        _dialog = dialog;
        _diffRepo = diffRepo;
        _exporter = exporter;
        _console = console;
        _loc = loc;
        _theme = theme;
        _chrome = chrome;
    }

    public string LabelKey => "menu.admin.export";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var palette = _theme.Active;

        var diff = await _diffRepo.GetLastAsync();
        if (diff is null)
        {
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["diff.empty"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var format = await _select.AskAsync(
            "menu.admin.export.format",
            new[]
            {
                new SelectChoice<DiffExportFormat>(DiffExportFormat.Pdf, "format.pdf"),
                new SelectChoice<DiffExportFormat>(DiffExportFormat.Csv, "format.csv"),
            },
            ct);

        var defaultName = $"diff-{diff.GeneratedAt:yyyyMMdd-HHmmss}.{(format == DiffExportFormat.Pdf ? "pdf" : "csv")}";
        var targetPath = await ResolveTargetPathAsync(_loc["menu.admin.export.dialog.title"].Value, defaultName, palette, ct);
        if (targetPath is null)
        {
            _console.MarkupLine($"[{palette.Muted}]cancelled[/]");
            _chrome.WaitForContinue();
            return;
        }

        try
        {
            var bytes = await _console
                .Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_loc["status.exporting"].Value, async _ =>
                {
                    var data = format == DiffExportFormat.Pdf
                        ? _exporter.ToPdf(diff, diff.DiffDataEntries)
                        : _exporter.ToCsv(diff, diff.DiffDataEntries);
                    await File.WriteAllBytesAsync(targetPath, data, ct);
                    return data;
                });

            _console.MarkupLine(
                $"[{palette.Success}]✓[/] [{palette.Highlight}]{Markup.Escape(Path.GetFullPath(targetPath))}[/] " +
                $"[{palette.Muted}]({bytes.Length} bytes)[/]");
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[{palette.Error}]export failed: {Markup.Escape(ex.Message)}[/]");
        }
        _chrome.WaitForContinue();
    }

    private async Task<string?> ResolveTargetPathAsync(string title, string defaultName, ThemePalette palette, CancellationToken ct)
    {
        if (_dialog.IsAvailable)
        {
            return await _dialog.ShowAsync(title, defaultName, ct);
        }

        _console.MarkupLine(
            $"[{palette.Muted}]{Markup.Escape(_loc["menu.admin.export.path.default", defaultName].Value)}[/]");
        var pathInput = await _text.AskAsync("menu.admin.export.path", ct: ct);
        var resolved = string.IsNullOrWhiteSpace(pathInput) ? defaultName : pathInput.Trim();

        if (File.Exists(resolved))
        {
            _console.MarkupLine(
                $"[{palette.Warning}]{Markup.Escape(_loc["menu.admin.export.overwrite.exists", resolved].Value)}[/]");
            if (!await _confirm.AskAsync("menu.admin.export.overwrite", defaultValue: false, ct))
            {
                return null;
            }
        }

        return resolved;
    }
}
