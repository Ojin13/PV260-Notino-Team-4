namespace Popocatepetl.Domain.Exceptions;

/// <summary>Thrown when a requested entity cannot be found in the data store.</summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Creates a new instance describing which entity and key were missing.</summary>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}
