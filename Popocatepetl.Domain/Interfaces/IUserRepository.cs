using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

/// <summary>Persistence contract for AppUser entities.</summary>
public interface IUserRepository
{
    /// <summary>Returns all users.</summary>
    Task<IEnumerable<AppUser>> GetAllAsync();

    /// <summary>Returns the user with the given id, or null if not found.</summary>
    Task<AppUser?> GetByIdAsync(Guid id);

    /// <summary>Persists a new user.</summary>
    Task AddAsync(AppUser user);

    /// <summary>Saves changes to an existing tracked user.</summary>
    Task UpdateAsync(AppUser user);

    /// <summary>Deletes the user with the given id.</summary>
    Task DeleteAsync(Guid id);
}
