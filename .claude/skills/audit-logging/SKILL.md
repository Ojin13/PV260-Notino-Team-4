---
name: audit-logging
description: Documents Popocatepetl's audit logging - how the IAuditableRequest marker and AuditLoggingBehavior pipeline wrap every user-visible MediatR request automatically. Use when adding/changing a command or query that should appear in the audit log, or when wondering why something does not.
argument-hint: "[command name, action label, or audit question]"
---

# Audit Logging

Every user-visible action is recorded in the `audit_logs` table — who did what, when, with what arguments, and whether it succeeded. The recording happens automatically as a **MediatR pipeline behavior**. Handlers don't call any audit API themselves.

## The two moving parts

### 1. The `IAuditableRequest` marker interface

`Popocatepetl.Application/Common/IAuditableRequest.cs`:

```csharp
public interface IAuditableRequest
{
    string ActionName { get; }
    string Detail { get; }
}
```

Any MediatR request (`IRequest<T>`) that implements this interface opts in to audit logging. Two contracts:

- **`ActionName`** — short PascalCase verb. Identifies the kind of action across all rows. Examples: `"DownloadLatestReport"`, `"ExportDiff"`, `"SendEmailReport"`, `"ChangeColorTheme"`, `"ViewAuditLogs"`.
- **`Detail`** — free-form string with the relevant parameters. Show enough to investigate later, but never put secrets, full payloads, or PII beyond what the audit screen needs.

Typical implementation on a command:

```csharp
public sealed record ExportDiffCommand(Guid DiffResultId, DiffExportFormat Format)
    : IAuditableRequest, IRequest<Result<ExportDiffResponse>>
{
    public string ActionName => "ExportDiff";
    public string Detail => $"DiffResultId: {DiffResultId}, Format: {Format}";
}
```

### 2. The `AuditLoggingBehavior` pipeline behavior

`Popocatepetl.Application/Behaviors/AuditLoggingBehavior.cs`. Wired into MediatR via:

```csharp
cfg.AddOpenBehavior(typeof(AuditLoggingBehavior<,>));
```

…in `Popocatepetl.Application/DependencyInjection.cs`. Generic constraint `where TRequest : IAuditableRequest` means it **only runs for requests that opted in** — non-auditable requests pass through untouched.

What it does:

```
try
{
    var response = await next();
    audit.Add(currentUser.Email, request.ActionName,
              UtcNow, success: true, request.Detail);
    return response;
}
catch (Exception ex)
{
    audit.Add(currentUser.Email, request.ActionName,
              UtcNow, success: false, ex.Message);
    throw;
}
```

The user identity comes from the registered `ICurrentUserContext` — the CLI's `CurrentUserContext` for CLI invocations, `HttpCurrentUserContext` for API ones.

## Mental model

- If a command implements `IAuditableRequest`, the audit row is written for you.
- If it doesn't, nothing is written.
- The behavior runs once per `IMediator.Send(...)`. It does **not** run for direct repository calls (which is why handlers should not bypass MediatR for cross-cutting work).
- It runs for both success and failure paths — but a `Result.Failure(...)` returned from the handler is logged as `success: true`. See [[error-handling]] for the rationale.

## When to make a request auditable

Implement `IAuditableRequest` when:

- The action is something an admin reviewing the audit log later would want to see.
- It mutates state, sends an email, exports data, downloads a report, changes a setting, or grants/revokes access.
- It's invoked from a menu item or an API endpoint a real user touches.

Skip it when:

- The request is purely internal plumbing (e.g., a query that backs the audit log view itself — circular).
- It's a fast read with no security-relevant context. Loading a list of recipients before showing a confirmation prompt doesn't need a row.

When in doubt, **mark it auditable**. The cost of a row is negligible; the cost of a missing one during an incident is high.

## Naming conventions

### `ActionName`

- PascalCase verb phrase.
- Stable — once a name is in the database, renaming it breaks historical reports. If you must rename, prefer adding a new name and leaving the old rows untouched.
- Singular and active: `"DownloadLatestReport"`, not `"reports/downloadLatest"` or `"DownloadedReports"`.

Existing names in the codebase to match style:
- `"DownloadLatestReport"`
- `"CreateReportsDiff"`
- `"ExportDiff"`
- `"SendEmailReport"`
- `"ViewAuditLogs"`

### `Detail`

- Show the inputs that scope the action. IDs, target counts, file formats, role transitions.
- Use a short, structured-looking format: `"key: value, key: value"`. The audit-log UI displays the raw string.
- Never include passwords, API keys, full email bodies, attachment contents, or anything that would embarrass you in a security review.
- Keep it under ~512 characters. The column is wider, but readable matters more than complete.

## Querying the audit log

The `GetAuditLogsQueryHandler` (`Popocatepetl.Application/Handlers/Admin/GetAuditLogsQueryHandler.cs`) is the entry point admins use to view and filter. It supports filtering by user email, action name, date, and success/failure — the admin menu wires those filters up through its own action class.

When **adding a new filter**, extend the query record + handler, not the audit table. The table itself should stay simple (append-only writes only).

## Writing tests around audit

`HandlerTestBase` builds a real MediatR pipeline, so the `AuditLoggingBehavior` runs for free in handler tests. Two patterns:

1. **Don't assert on audit in every test.** It runs whether you check or not. Asserting on it in every handler test creates noise.
2. **Do assert on it in dedicated audit tests.** Inject a `Mock<IAuditLogRepository>` via `MockedIocBuilder` and `Verify` that `AddAsync` was called with the right `success` flag, `ActionName`, and `Detail` for each branch. One test per audited handler is enough; the behavior itself is tested separately.

A `Result.Failure` returned by the handler still produces a `success: true` audit row. If a test asserts otherwise, it's wrong — fix the test or fix the [[error-handling]] design.

## Anti-patterns

- **Manually calling `IAuditLogRepository.AddAsync(...)` from a handler.** Implement `IAuditableRequest` instead. Manual calls duplicate rows and bypass the success/failure logic.
- **Putting `ActionName` in localized resources.** It's an internal identifier, not user-facing copy. Keep it in English in the request record.
- **Reusing the same `ActionName` for unrelated commands.** Each command gets its own. If two flows really are the same action, they should be the same command.
- **Catching an exception inside the handler just to record audit failure manually.** The behavior already does that.

## Adding audit to an existing request — checklist

- [ ] Add `IAuditableRequest` to the request record's interfaces
- [ ] Implement `ActionName` and `Detail` getters
- [ ] Verify the request is sent via `IMediator.Send(...)` (not called directly by some short-circuit)
- [ ] Update or add tests to cover the audit success path and the audit failure path

## Related skills

- [[error-handling]] — why `Result.Failure` produces a `success: true` audit row
- [[clean-architecture]] — pipeline behaviors are an Application-layer concern
- [[testing]] — `HandlerTestBase` keeps the behavior live
- [[add-feature]] — when to opt into audit on a new feature
