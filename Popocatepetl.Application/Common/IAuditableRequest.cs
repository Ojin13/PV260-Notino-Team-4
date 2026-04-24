namespace Popocatepetl.Application.Common;

public interface IAuditableRequest
{
    string Email { get; }
    string ActionName { get; }
    string Detail { get; }
}
