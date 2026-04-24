using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

public interface IDiffResultRepository : IRepository<DiffResult>
{
    Task<DiffResult?> GetLastAsync();
}
