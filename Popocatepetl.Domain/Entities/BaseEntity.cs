namespace Popocatepetl.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }

    protected BaseEntity() { }
}
