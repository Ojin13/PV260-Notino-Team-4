using MediatR;
using Microsoft.AspNetCore.Mvc;
using Popocatepetl.Application.Reports;

namespace Popocatepetl.Api.Controllers;

/// <summary>Endpoints for admin report actions.</summary>
[ApiController]
[Route("api/admin/reports")]
public sealed class AdminReportsController(ISender sender) : ControllerBase
{
    /// <summary>Downloads the latest ARK report and stores it.</summary>
    [HttpPost("download-latest")]
    [ProducesResponseType<DownloadLatestArkReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadLatest(CancellationToken ct)
    {
        var result = await sender.Send(new DownloadLatestArkReportCommand(), ct);
        if (!result.IsSuccess)
        {
            return result.ErrorMessage == "Only admins can download reports."
                ? StatusCode(StatusCodes.Status403Forbidden, result.ErrorMessage)
                : StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage);
        }

        return Ok(result.Value);
    }
}
