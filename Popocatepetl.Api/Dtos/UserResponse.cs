using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Api.Dtos;

public sealed record UserResponse(Guid Id, string Email, DateTime CreatedAt)
{
    public static UserResponse FromEntity(AppUser user) =>
        new(user.Id, user.Email, user.CreatedAt);
}
