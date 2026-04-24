using MediatR;
using Microsoft.AspNetCore.Mvc;
using Popocatepetl.Application.Commands;
using Popocatepetl.Application.Queries.UserRole;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Api.Controllers
{
    [ApiController]
    [Route("api/user-role")]
    public class UserRoleController(ISender sender) : ControllerBase
    {
        [HttpGet("diff-report")]
        [ProducesResponseType<IReadOnlyList<AppUser>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDiffReport(CancellationToken ct)
        {
            var result = await sender.Send(new GetDiffReportQuery(), ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("diff-report")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateDiffReport([FromBody] CreateReportsDiffRequest request, CancellationToken ct)
        {
            await sender.Send(new CreateReportsDiffCommand(request.BaselineReportId, request.CurrentReportId, request.Email), ct);
            return NoContent();
        }
    }

    public record CreateReportsDiffRequest(Guid BaselineReportId, Guid CurrentReportId, string Email);
}
