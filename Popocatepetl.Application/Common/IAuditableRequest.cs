namespace Popocatepetl.Application.Common;

public interface IAuditableRequest
{
    string ActionName { get; }
}
