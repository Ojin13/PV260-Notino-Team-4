using MediatR;
using Microsoft.AspNetCore.Mvc;
using Popocatepetl.Api.Dtos;
using Popocatepetl.Application.Users;

namespace Popocatepetl.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Returns all registered users.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await sender.Send(new GetAllUsersQuery(), ct);
        return Ok(users.Select(UserResponse.FromEntity).ToList());
    }

    /// <summary>Returns the user with the specified ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), ct);
        return user is null ? NotFound() : Ok(UserResponse.FromEntity(user));
    }

    /// <summary>Creates a new user.</summary>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = await sender.Send(new CreateUserCommand(request.Email), ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, UserResponse.FromEntity(user));
    }

    /// <summary>Updates the email of an existing user.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        await sender.Send(new UpdateUserCommand(id, request.Email), ct);
        return NoContent();
    }

    /// <summary>Deletes the user with the specified ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
}

public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
