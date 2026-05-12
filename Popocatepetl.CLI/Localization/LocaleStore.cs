using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.CLI.Localization;

public sealed class LocaleStore
{
    public const string DefaultLocale = "en";

    public static readonly IReadOnlyList<string> Supported = new[] { "en", "cs", "sk" };

    private readonly IAppSettingRepository _settings;

    public LocaleStore(IAppSettingRepository settings) => _settings = settings;

    public async Task<string> GetActiveAsync(CancellationToken ct = default)
    {
        var stored = await _settings.GetAsync(AppSettingType.Language);
        return Supported.Contains(stored) ? stored! : DefaultLocale;
    }

    public Task SetActiveAsync(string cultureTag, CancellationToken ct = default) =>
        _settings.SetAsync(AppSettingType.Language, cultureTag);
}
