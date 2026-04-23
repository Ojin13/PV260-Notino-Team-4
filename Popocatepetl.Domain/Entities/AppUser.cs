namespace Popocatepetl.Domain.Entities;

/// <summary>Represents a registered user of the application.</summary>
public class AppUser
{
    /// <summary>Unique identifier for the user.</summary>
    public Guid Id { get; init; }

    /// <summary>The user's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>When the user account was created.</summary>
    public DateTime CreatedAt { get; init; }

    private AppUser()
    {
    }

    public static AppUser Create(
        string email,
        DateTime? createdAt = null,
        Guid? id = null)
    {
        return new AppUser
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }
}
