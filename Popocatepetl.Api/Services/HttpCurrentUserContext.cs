using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Api.Services;

/// <summary>
/// Reads the current user's identity lazily from ASP.NET Core request headers.
/// Registered as Scoped so it is tied to the lifetime of a single HTTP request.
/// </summary>
public sealed class HttpCurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    public string Email =>
        accessor.HttpContext?.Request.Headers["X-User-Email"].ToString() ?? string.Empty;

    public UserRole Role =>
        Enum.TryParse<UserRole>(
            accessor.HttpContext?.Request.Headers["X-User-Role"].ToString(),
            ignoreCase: true,
            out var role)
            ? role
            : UserRole.User;
}
