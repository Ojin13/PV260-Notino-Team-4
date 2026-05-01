using System.Globalization;
using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.CLI.Menus.Settings;

public sealed class LanguageMenuAction : IMenuAction
{
    private readonly ISelectPrompt _select;
    private readonly LocaleStore _store;
    private readonly LocaleState _state;
    private readonly IAuditLogRepository _audit;
    private readonly ICurrentUserContext _session;

    public LanguageMenuAction(
        ISelectPrompt select,
        LocaleStore store,
        LocaleState state,
        IAuditLogRepository audit,
        ICurrentUserContext session)
    {
        _select = select;
        _store = store;
        _state = state;
        _audit = audit;
        _session = session;
    }

    public string LabelKey => "menu.language.change";

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var currentTag = _state.Current.Name;
        var choices = LocaleStore.Supported
            .Select(tag => new SelectChoice<string>(tag, $"language.{tag}", IsCurrent: tag == currentTag))
            .ToList();

        var picked = await _select.AskAsync("menu.language.title", choices, ct);

        await _store.SetActiveAsync(picked, ct);
        _state.Set(CultureInfo.GetCultureInfo(picked));

        await _audit.AddAsync(AuditLog.Create(
            _session.Email,
            "ChangeLanguage",
            DateTime.UtcNow,
            wasSuccessful: true,
            details: $"Locale={picked}"));
    }
}
