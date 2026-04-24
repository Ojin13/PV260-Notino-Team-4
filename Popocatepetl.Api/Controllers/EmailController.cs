using MediatR;
using Microsoft.AspNetCore.Mvc;
using Popocatepetl.Api.Dtos;
using Popocatepetl.Application.Commands;

namespace Popocatepetl.Api.Controllers;

/// <summary>Endpoints for sending outbound emails.</summary>
[ApiController]
[Route("api/email")]
public sealed class EmailController(ISender sender) : ControllerBase
{
    /// <summary>Sends the latest generated ARKK diff as an email attachment to the specified recipients.</summary>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Send([FromBody] SendEmailRequest request, CancellationToken ct)
    {
        await sender.Send(new SendEmailCommand(request.Recipients, request.Format), ct);
        return Accepted();
    }
}