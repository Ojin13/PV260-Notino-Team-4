namespace Popocatepetl.Domain.Entities;

/// <summary>Records a single auditable action performed by a user.</summary>
public class AuditLog
{
    /// <summary>Unique identifier for the audit entry.</summary>
    public Guid Id { get; init; }

    /// <summary>Email of the user who performed the action.</summary>
    public string UserEmail { get; init; } = string.Empty;

    /// <summary>Name of the action that was performed, e.g. "DownloadReport".</summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>When the action occurred.</summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>Whether the action completed successfully.</summary>
    public bool WasSuccessful { get; init; }

    /// <summary>Optional extra context or error details about the action.</summary>
    public string? Details { get; init; }

    private AuditLog()
    {
    }

    public static AuditLog Create(
        string userEmail,
        string action,
        DateTime? occurredAt,
        bool wasSuccessful,
        string? details
    )
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserEmail = userEmail,
            Action = action,
            OccurredAt = occurredAt ?? DateTime.UtcNow,
            WasSuccessful = wasSuccessful,
            Details = details
        };
    }
    
    
}
