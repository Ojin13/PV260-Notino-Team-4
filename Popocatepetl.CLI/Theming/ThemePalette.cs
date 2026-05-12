namespace Popocatepetl.CLI.Theming;

public sealed record ThemePalette(
    string Id,
    string DisplayKey,
    string Heading,
    string Highlight,
    string Muted,
    string Success,
    string Warning,
    string Error,
    string Border);
