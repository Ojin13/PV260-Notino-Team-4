namespace Popocatepetl.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; private protected set; }
    public DateTime CreatedAt { get; private protected set; }

    protected BaseEntity() { }
}
