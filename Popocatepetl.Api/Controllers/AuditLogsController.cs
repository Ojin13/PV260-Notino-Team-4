using MediatR;
using Microsoft.AspNetCore.Mvc;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Queries.Admin;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Enums;

namespace Popocatepetl.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
public sealed class AuditLogsController(ISender sender, ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AuditLog>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] string? email,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? action,
        CancellationToken ct)
    {
        if (currentUser.Role != UserRole.Admin)
            return Forbid();

        var logs = await sender.Send(new GetAuditLogsQuery(email, from, to, action), ct);
        return Ok(logs);
    }
}
