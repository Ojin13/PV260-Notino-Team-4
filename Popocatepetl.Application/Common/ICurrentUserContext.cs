using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Application.Common;

/// <summary>Provides identity information about the currently active user.</summary>
public interface ICurrentUserContext
{
    /// <summary>Email address of the current user.</summary>
    string Email { get; }

    /// <summary>Role assigned to the current user.</summary>
    UserRole Role { get; }
}
