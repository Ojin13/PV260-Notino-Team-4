using MediatR;
using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Application.Queries.UserRole
{
    public record GetDiffReportQuery(): IRequest<IReadOnlyList<DiffData>?>;
}
