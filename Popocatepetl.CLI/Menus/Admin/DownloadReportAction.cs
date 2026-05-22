using MediatR;
using Microsoft.Extensions.Localization;
using Popocatepetl.Application.Reports;
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
    private readonly LocaleState _locale;

    public DownloadReportAction(
        ISender sender,
        IReportRepository reports,
        IConfirmPrompt confirm,
        ITextPrompt text,
        IFileSaveDialog dialog,
        IAnsiConsole console,
        IStringLocalizer<CliStrings> loc,
        ThemeApplier theme,
        MenuChrome chrome,
        LocaleState locale)
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
        _locale = locale;
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

        _locale.AlignCurrentThread();
        if (!result.IsSuccess)
        {
            _console.MarkupLine($"[{palette.Error}]{Markup.Escape(result.ErrorMessage ?? _loc["error.generic"].Value)}[/]");
            _chrome.WaitForContinue();
            return;
        }

        var r = result.Value!;
        _console.MarkupLine(
            $"[{palette.Success}]✓[/] [{palette.Highlight}]{Markup.Escape(r.FileName)}[/] " +
            $"[{palette.Muted}]({r.UploadedAt:u}, id={r.ReportId})[/]");

        if (r.PreviousReportFound)
        {
            _console.MarkupLine($"[{palette.Success}]✓ {Markup.Escape(_loc["status.diff.recalculated"].Value)}[/]");
        }
        else
        {
            _console.MarkupLine($"[{palette.Warning}]{Markup.Escape(_loc["status.diff.first"].Value)}[/]");
        }

        if (await _confirm.AskAsync("menu.admin.download.savecopy.prompt", defaultValue: false, ct))
        {
            await SaveLocalCopyAsync(r.ReportId, r.FileName, palette, ct);
        }

        _chrome.WaitForContinue();
    }

    private async Task SaveLocalCopyAsync(Guid reportId, string fileName, ThemePalette palette, CancellationToken ct)
    {
        _locale.AlignCurrentThread();
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
            _console.MarkupLine($"[{palette.Muted}]{Markup.Escape(_loc["status.cancelled"].Value)}[/]");
            return;
        }

        try
        {
            var saved = await _reports.GetByIdAsync(reportId);
            if (saved is null)
            {
                _console.MarkupLine($"[{palette.Error}]{Markup.Escape(_loc["error.report.missing"].Value)}[/]");
                return;
            }
            await File.WriteAllTextAsync(path, saved.RawContent, ct);
            _console.MarkupLine(
                $"[{palette.Success}]✓ {Markup.Escape(_loc["status.saved"].Value)} →[/] [{palette.Highlight}]{Markup.Escape(Path.GetFullPath(path))}[/]");
        }
        catch (Exception ex)
        {
            _console.MarkupLine($"[{palette.Error}]{Markup.Escape(_loc["error.save", ex.Message].Value)}[/]");
        }
    }
}
