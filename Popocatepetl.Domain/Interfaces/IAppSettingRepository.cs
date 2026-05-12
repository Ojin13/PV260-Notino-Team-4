using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Domain.Interfaces;

public interface IAppSettingRepository
{
    Task<string?> GetAsync(AppSettingType type);
    Task SetAsync(AppSettingType type, string value);
}
