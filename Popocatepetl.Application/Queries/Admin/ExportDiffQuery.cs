using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Application.Dtos;

namespace Popocatepetl.Application.Queries.Admin;

public record ExportDiffQuery(DiffExportFormat Format) : IAuditableRequest, IRequest<ExportDiffResponse?>
{
    public string ActionName => "ExportDiff";
    public string Detail => Format.ToString();
}
