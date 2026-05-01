using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Commands;

/// <summary>Sends the latest generated diff to the specified recipients as a file attachment.</summary>
public record SendEmailCommand(
    IReadOnlyList<string> Recipients,
    DiffExportFormat Format) : IAuditableRequest, IRequest<Unit>
{
    public string ActionName => "SendEmailReport";

    public string Detail =>
        $"Format={Format}, Recipients=[{string.Join(", ", Recipients ?? Array.Empty<string>())}]";
}
