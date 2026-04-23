using Popocatepetl.Domain.Entities;

namespace Popocatepetl.Domain.Interfaces;

public interface IDiffResultRepository
{
    Task<DiffResult?> GetLastAsync();

    Task CreateAsync(DiffResult diffResult);
}
