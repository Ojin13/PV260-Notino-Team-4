namespace Popocatepetl.Domain.Entities;

/// <summary>A key-value application configuration entry.</summary>
public class AppSetting
{
    /// <summary>Unique key identifying the setting.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>The value associated with the key.</summary>
    public string Value { get; set; } = string.Empty;

    private AppSetting()
    {
    }

    public static AppSetting Create(string key, string value)
    {
        return new AppSetting
        {
            Key = key,
            Value = value
        };
    }
}
