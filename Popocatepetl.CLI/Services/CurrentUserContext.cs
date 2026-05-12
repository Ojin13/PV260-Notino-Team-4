using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.CLI.Services;

/// <summary>
/// Mutable session context that holds the identity of the authenticated user.
/// Populated after the auth pipeline succeeds; injected as Scoped so all handlers
/// in the same request share the same instance.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.None;
}
