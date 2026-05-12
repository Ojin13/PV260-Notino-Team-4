using System.Globalization;

namespace Popocatepetl.CLI.Localization;

public sealed class LocaleState
{
    private CultureInfo _culture = CultureInfo.GetCultureInfo(LocaleStore.DefaultLocale);

    public CultureInfo Current => _culture;

    public void Set(CultureInfo culture)
    {
        _culture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public void AlignCurrentThread()
    {
        if (!Equals(CultureInfo.CurrentUICulture, _culture))
        {
            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _culture;
        }
    }
}
