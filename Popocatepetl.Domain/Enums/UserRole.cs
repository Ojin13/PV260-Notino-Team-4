namespace Popocatepetl.Domain.Enums;

/// <summary>Defines the access level granted to an application user.</summary>
public enum UserRole
{
    /// <summary>No role assigned.</summary>
    None,

    /// <summary>Standard user with read access.</summary>
    User,

    /// <summary>Elevated user who can upload and compare reports.</summary>
    PowerUser,

    /// <summary>Full administrative access.</summary>
    Admin,
}
