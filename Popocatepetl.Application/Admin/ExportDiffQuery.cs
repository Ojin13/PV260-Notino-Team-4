using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Admin;

public record ExportDiffQuery(DiffExportFormat Format) : IAuditableRequest, IRequest<ExportDiffResponse?>
{
    public string ActionName => "ExportDiff";
    public string Detail => Format.ToString();
}
