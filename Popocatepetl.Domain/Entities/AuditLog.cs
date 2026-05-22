namespace Popocatepetl.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserEmail { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public bool WasSuccessful { get; private set; }
    public string? Details { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(string userEmail, string action, DateTime? occurredAt, bool wasSuccessful, string? details) =>
        new AuditLog
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UserEmail = userEmail,
            Action = action,
            OccurredAt = occurredAt ?? DateTime.UtcNow,
            WasSuccessful = wasSuccessful,
            Details = details
        };
}
