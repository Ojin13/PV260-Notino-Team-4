using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Entities;

public class AppSetting
{
    public AppSettingType Type { get; private set; }
    public string Value { get; private set; } = string.Empty;

    private AppSetting() { }

    public static AppSetting Create(AppSettingType type, string value) =>
        new AppSetting
        {
            Type = type,
            Value = value
        };

    public void UpdateValue(string value)
    {
        Value = value;
    }
}
