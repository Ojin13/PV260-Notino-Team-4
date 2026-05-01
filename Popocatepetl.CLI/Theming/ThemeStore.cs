using Popocatepetl.Domain.Enums;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.CLI.Theming;

public sealed class ThemeStore
{
    private readonly IAppSettingRepository _settings;

    public ThemeStore(IAppSettingRepository settings) => _settings = settings;

    public async Task<ThemePalette> GetActiveAsync(CancellationToken ct = default)
    {
        var stored = await _settings.GetAsync(AppSettingType.ColorScheme);
        return BuiltInPalettes.ResolveOrDefault(stored);
    }

    public Task SetActiveAsync(string paletteId, CancellationToken ct = default) =>
        _settings.SetAsync(AppSettingType.ColorScheme, paletteId);
}
