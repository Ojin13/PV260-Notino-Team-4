using MediatR;
using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Commands.Reports;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Interfaces;
using Spectre.Console;

namespace Popocatepetl.CLI.Menus.Admin;

public sealed class DownloadReportAction : IMenuAction
{
    private readonly ISender _sender;
    private readonly IReportRepository _reports;
    private readonly IConfirmPrompt _confirm;
    private readonly ITextPrompt _text;
    private readonly IFileSaveDialog _dialog;
    private readonly IAnsiConsole _console;
    private readonly IStringLocalizer<CliStrings> _loc;
    private readonly ThemeApplier _theme;
    private readonly MenuChrome _chrome;

    public DownloadReportAction(
        ISender sender,
        IReportRepository reports,
        IConfirmPrompt confirm,
        ITextPrompt text,
        IFileSaveDialog dialog,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme,
        MenuChrome chrome)
    {
        _sender = sender;
        _reports = reports;
        _confirm = confirm;
        _text = text;
        _dialog = dialog;
        _console = console;
        _loc = loc;
        _theme = theme;
        _chrome = chrome;
    }

    public string LabelKey => "menu.admin.download";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var palette = _theme.Active;

        var result = await _console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(_loc["status.downloading"].Value, async _ =>
                await _sender.Send(new DownloadLatestArkReportCommand(), ct));

        if (!result.IsSuccess)
        {
            _console.MarkupLine($"[{palette.Error}]{Markup.Escape(result.ErrorMessage ?? "error")}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var r = result.Value!;
        _console.MarkupLine(
            $"[{palette.Success}]✓[/] [{palette.Highlight}]{Markup.Escape(r.FileName)}[/] " +
            $"[{palette.Muted}]({r.UploadedAt:u}, id={r.ReportId})[/]");

        if (r.PreviousReportFound)
        {
            _console.MarkupLine($"[{palette.Success}]✓ diff recalculated against previous report[/]");
        }
        else
        {
            _console.MarkupLine($"[{palette.Warning}](no previous report — diff will appear after a second download)[/]");
        }

        if (await _confirm.AskAsync("menu.admin.download.savecopy.prompt", defaultValue: false, ct))
        {
            await SaveLocalCopyAsync(r.ReportId, r.FileName, palette, ct);
        }

        _chrome.WaitForContinue();
    }

    private async Task SaveLocalCopyAsync(Guid reportId, string fileName, ThemePalette palette, CancellationToken ct)
    {
        var title = _loc["menu.admin.download.dialog.title"].Value;
        string? path;

        if (_dialog.IsAvailable)
        {
            path = await _dialog.ShowAsync(title, fileName, ct);
        }
        else
        {
            _console.MarkupLine(
                $"[{palette.Muted}]{Markup.Escape(_loc["menu.admin.export.path.default", fileName].Value)}[/]");
            var pathInput = await _text.AskAsync("menu.admin.download.path", ct: ct);
            path = string.IsNullOrWhiteSpace(pathInput) ? fileName : pathInput.Trim();
        }

        if (string.IsNullOrEmpty(path))
        {
            _console.MarkupLine($"[{palette.Muted}]cancelled[/]");
            return;
        }

        try
        {
            var saved = await _reports.GetByIdAsync(reportId);
            if (saved is null)
            {
                _console.MarkupLine($"[{palette.Error}]report disappeared from database[/]");
                return;
            }
            await File.WriteAllTextAsync(path, saved.RawContent, ct);
            _console.MarkupLine(
                $"[{palette.Success}]✓ saved copy →[/] [{palette.Highlight}]{Markup.Escape(Path.GetFullPath(path))}[/]");
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[{palette.Error}]save failed: {Markup.Escape(ex.Message)}[/]");
        }
    }
}
