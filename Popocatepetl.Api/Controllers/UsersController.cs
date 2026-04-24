using MediatR;
using Microsoft.AspNetCore.Mvc;
using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Queries.Users;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    /// <summary>Returns all registered users.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AppUser>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetAllUsersQuery(), ct));

    /// <summary>Returns the user with the specified ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AppUser>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), ct);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Creates a new user.</summary>
    [HttpPost]
    [ProducesResponseType<AppUser>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = await sender.Send(new CreateUserCommand(request.Email), ct);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
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
    public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteUserRequest request, CancellationToken ct)
    {
        await sender.Send(new DeleteUserCommand(id, request.Email), ct);
        return NoContent();
    }
}

public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
public record DeleteUserRequest(string Email);
