namespace Popocatepetl.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserEmail { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public bool WasSuccessful { get; init; }
    public string? Details { get; init; }

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
