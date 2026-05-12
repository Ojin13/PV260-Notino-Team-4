using Spectre.Console;

namespace Popocatepetl.CLI.Theming;

public static class PaletteSpectreExtensions
{
    public static Style HeadingStyle(this ThemePalette p) => Style.Parse(p.Heading);
    public static Style HighlightStyle(this ThemePalette p) => Style.Parse(p.Highlight);
    public static Style MutedStyle(this ThemePalette p) => Style.Parse(p.Muted);
    public static Style SuccessStyle(this ThemePalette p) => Style.Parse(p.Success);
    public static Style WarningStyle(this ThemePalette p) => Style.Parse(p.Warning);
    public static Style ErrorStyle(this ThemePalette p) => Style.Parse(p.Error);
    public static Style BorderStyle(this ThemePalette p) => Style.Parse(p.Border);
}
