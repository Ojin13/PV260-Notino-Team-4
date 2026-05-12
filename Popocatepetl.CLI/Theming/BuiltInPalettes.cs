namespace Popocatepetl.CLI.Theming;

public static class BuiltInPalettes
{
    public const string DefaultId = "default";

    public static readonly ThemePalette Default = new(
        DefaultId, "theme.default",
        Heading: "white", Highlight: "cyan1", Muted: "grey",
        Success: "green", Warning: "yellow", Error: "red", Border: "grey");

    public static readonly ThemePalette HighContrast = new(
        "high-contrast", "theme.high-contrast",
        Heading: "white", Highlight: "yellow", Muted: "white",
        Success: "lime", Warning: "yellow", Error: "red1", Border: "white");

    public static readonly ThemePalette Solarized = new(
        "solarized", "theme.solarized",
        Heading: "blue1", Highlight: "yellow3", Muted: "grey50",
        Success: "green3", Warning: "orange3", Error: "red3", Border: "grey50");

    public static readonly ThemePalette Monokai = new(
        "monokai", "theme.monokai",
        Heading: "magenta1", Highlight: "green1", Muted: "grey50",
        Success: "green1", Warning: "orange1", Error: "red1", Border: "grey39");

    public static readonly IReadOnlyList<ThemePalette> All = new[]
    {
        Default, HighContrast, Solarized, Monokai,
    };

    public static ThemePalette ResolveOrDefault(string? id) =>
        All.FirstOrDefault(p => p.Id == id) ?? Default;
}
