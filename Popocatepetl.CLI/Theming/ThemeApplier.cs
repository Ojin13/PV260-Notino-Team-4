namespace Popocatepetl.CLI.Theming;

public sealed class ThemeApplier
{
    public ThemePalette Active { get; private set; } = BuiltInPalettes.Default;

    public void Apply(ThemePalette palette) => Active = palette;
}
