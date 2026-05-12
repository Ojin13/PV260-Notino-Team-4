using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Entities;

public class AppSetting
{
    public AppSettingType Type { get; init; }
    public string Value { get; set; } = string.Empty;

    private AppSetting() { }

    public static AppSetting Create(AppSettingType type, string value) =>
        new AppSetting
        {
            Type = type,
            Value = value
        };
}
