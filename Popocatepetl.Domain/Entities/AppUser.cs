namespace Popocatepetl.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Email { get; private set; } = string.Empty;

    private AppUser() { }

    public static AppUser Create(string email, DateTime? createdAt = null, Guid? id = null) =>
        new AppUser
        {
            Id = id ?? Guid.NewGuid(),
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Email = email
        };

    public void UpdateEmail(string email)
    {
        Email = email;
    }
}
