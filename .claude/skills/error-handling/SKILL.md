---
name: error-handling
description: Documents how Popocatepetl reports failures - the Result<T> envelope for expected failures, exceptions (including NotFoundException) for unexpected ones, and how the AuditLoggingBehavior records both. Use when a handler/action needs to fail in a recoverable way, or when picking between Result.Failure and throw.
argument-hint: "[scenario, exception type, or 'should I throw']"
---

# Error Handling

Two patterns coexist in this codebase, and they are not interchangeable.

| Pattern | Use for | What happens |
| --- | --- | --- |
| **`Result<T>` envelope** | Expected failures the user can recover from | Handler returns `Result.Failure("...")`. Audit row is written. Presentation displays the message. |
| **Exception (`throw`)** | Unexpected failures, programmer bugs | Audit behavior catches it, records a failure row, **re-throws**. Bubbles up to the presentation. |

Default to `Result<T>`. Reach for exceptions only when the failure means the application's invariants have been violated.

## The Result&lt;T&gt; envelope

`Popocatepetl.Application/Common/Result.cs`:

```csharp
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
}
```

Use it for:

- **Authorization failures.** "Only admins can download reports."
- **Validation failures.** "Email is required.", "Recipient list cannot exceed 100."
- **Missing inputs.** "No baseline report exists yet — download one first."
- **Domain rule violations.** "Cannot send a report before a diff has been computed."
- **External-service failures the user can retry.** "ARK report download failed, try again."

A handler returning `Result.Failure(...)` is the normal way to communicate "this is not allowed right now." Throwing for these cases makes the audit log harder to read, makes the API's response shape inconsistent, and forces callers into try/catch when they should be doing `if (!result.IsSuccess)`.

### Construct, return, render

```csharp
// in a handler
if (currentUserContext.Role != UserRole.Admin)
    return Result<…>.Failure("Only admins can download reports.");

return Result<…>.Success(new DownloadLatestArkReportResponse(...));

// in a CLI action
var result = await mediator.Send(command, ct);
if (!result.IsSuccess)
{
    console.MarkupLine($"[red]{Markup.Escape(result.ErrorMessage)}[/]");
    return;
}
// render result.Value...

// in a REST controller
return result.IsSuccess
    ? Ok(result.Value)
    : BadRequest(result.ErrorMessage);
```

### Localizing the message

If the message will be shown to a CLI user, prefer a localized string key returned by the handler, then resolve it at the presentation edge. For simple admin-only operations where the audience is small, English messages are accepted today. Be consistent within a feature.

## Exceptions

Use these for **unexpected** failures. The pattern is to throw a specific exception type so the audit behavior and the global error handlers can pick it up.

### `NotFoundException`

`Popocatepetl.Domain/Exceptions/NotFoundException.cs`. Throw when an internal invariant says an entity must exist (an ID was passed from trusted code), but the database can't find it. This is a bug-class signal — clients should never produce it by sending random IDs.

```csharp
var report = await reportRepository.GetByIdAsync(id)
    ?? throw new NotFoundException($"Report {id} not found.");
```

If the lookup could legitimately fail because of a user-supplied ID (e.g., a stale frontend), return `Result.Failure` instead.

### General exceptions

For any other unexpected failure (database connection dropped, file system out of space, third-party SDK panic), let the exception propagate. `AuditLoggingBehavior` will record a failure row and re-throw. Don't catch-and-swallow; that hides bugs.

## The audit interplay

`AuditLoggingBehavior<TRequest, TResponse>` (`Popocatepetl.Application/Behaviors/AuditLoggingBehavior.cs`) wraps every `IAuditableRequest` with:

```
try
{
    var response = await next();
    audit.Add(success: true, request.ActionName, request.Detail);
    return response;
}
catch (Exception ex)
{
    audit.Add(success: false, request.ActionName, ex.Message);
    throw;
}
```

Two important consequences:

1. **A `Result.Failure(...)` is recorded as `success: true`** because the handler returned normally. If you want the audit log to flag the failure, throw — but only if it warrants exception treatment per the rules above. This is intentional: business-rule rejections are part of normal flow, not anomalies.
2. **Thrown exceptions are recorded as `success: false`** with the exception's `Message`. Don't put internal stack traces or secrets in exception messages — they end up in the database.

If you genuinely need an audit failure row for a `Result.Failure` (rare), promote the relevant condition to a throw with a dedicated exception type. Don't paper over it with both.

## Don't

- **Don't catch and rethrow** without adding information. `catch (Exception ex) { throw ex; }` resets the stack trace; even `catch (Exception ex) { throw; }` is usually pointless.
- **Don't return `null`** to signal failure when `Result<T>` is available.
- **Don't bake error messages into log strings.** Use the `ILogger` API with parameters: `logger.LogError(ex, "Failed to send report to {Email}", email)`.
- **Don't expose raw exception details to end users.** A CLI render should show the curated `Result.ErrorMessage`, not `ex.ToString()`.
- **Don't introduce a new exception type** unless an existing one doesn't fit. Most cases want `Result.Failure` instead.

## Quick decision flowchart

```
Is this failure expected as part of normal flow?
├── Yes → Result.Failure
│         (e.g., "user is not an admin", "email list is empty")
└── No → throw
          ├── ID came from trusted code but row is missing → NotFoundException
          ├── Domain invariant violated → DomainException (add one if needed)
          └── Anything else → let it bubble; the global handler/audit
                              will catch it
```

## Related skills

- [[audit-logging]] — what gets logged and how
- [[clean-architecture]] — where handlers live and what they return
- [[testing]] — asserting on `Result` shape and verifying audit interactions
