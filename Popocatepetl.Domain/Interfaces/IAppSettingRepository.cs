namespace Popocatepetl.Domain.Interfaces;

/// <summary>Persistence contract for application key-value settings.</summary>
public interface IAppSettingRepository
{
    /// <summary>Returns the value for the given key, or null if the key does not exist.</summary>
    Task<string?> GetAsync(string key);

    /// <summary>Creates or overwrites the setting with the given key and value.</summary>
    Task SetAsync(string key, string value);
}
