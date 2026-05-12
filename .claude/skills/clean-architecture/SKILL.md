---
name: clean-architecture
description: Documents the four-layer Clean Architecture used by Popocatepetl - dependency rules, what lives where, and how a request flows from a Spectre menu pick through MediatR into the database. Read this first when starting any non-trivial change.
argument-hint: "[layer name, request flow question, or 'where does X go']"
---

# Clean Architecture

Popocatepetl is split into four layers. The dependency direction is strict and one-way. Breaking it is the most common way to make code unmaintainable here, so this skill exists to keep that from happening.

## The layers

```
Presentation        Popocatepetl.CLI     Popocatepetl.Api
                         (Spectre menu)       (REST controllers)
                              │                    │
                              └────────┬───────────┘
                                       ▼
Infrastructure      Popocatepetl.Infrastructure
                         EF Core (SQLite), Resend email,
                         ArkReportClient (HTTP), QuestPDF export,
                         repositories implementing Domain ports
                                       │
                                       ▼
Application         Popocatepetl.Application
                         MediatR commands/queries + handlers,
                         AuditLoggingBehavior, Result<T>,
                         ICurrentUserContext, IAuditableRequest
                                       │
                                       ▼
Domain              Popocatepetl.Domain
                         Entities, Enums, Value Objects, Events,
                         Interfaces (ports) — no outward deps
```

Lower layers never reference higher ones. Domain depends on nothing. Application depends on Domain only. Infrastructure depends on Application + Domain. Presentation depends on all three.

## What lives in each layer

### Popocatepetl.Domain
- `Entities/` — `Report`, `DiffResult`, `DiffData`, `AppUser`, `AuditLog`, `AppSetting`, `MailAttachment`, `BaseEntity`
- `Enums/` — `UserRole`, `AppSettingType`, `ShareDiffType`
- `Interfaces/` — every port Infrastructure implements: `IArkReportClient`, `IEmailService`, `IDiffCalculator`, `IDiffExporter`, `ICsvParser`, and all `I*Repository` types
- `ValueObjects/`, `Events/`, `Exceptions/`

**Rule:** plain C# only. No `using Microsoft.EntityFrameworkCore`. No `using Spectre.Console`. No HTTP types. If you can't write the file with only `System.*` references plus other Domain types, it does not belong here.

### Popocatepetl.Application
- `Commands/{Area}/XxxCommand.cs` — state-changing request records (`IRequest<Result<T>>` or `IRequest<Unit>`)
- `Queries/{Area}/XxxQuery.cs` — read request records
- `Handlers/{Area}/XxxCommandHandler.cs` — `IRequestHandler<TReq,TRes>` implementations
- `Behaviors/AuditLoggingBehavior.cs` — MediatR pipeline behavior for the audit log
- `Common/` — `Result<T>`, `IAuditableRequest`, `ICurrentUserContext`, `DiffExportFormat`
- `Dtos/` — request/response shapes shared between handlers and presentation
- `DependencyInjection.cs` — `AddApplication()` extension that registers MediatR + behaviors

**Rule:** depend on Domain interfaces only. Never `using Popocatepetl.Infrastructure` here. If you need a database, an HTTP client, or a file system, depend on a Domain port and let DI resolve the Infrastructure implementation.

### Popocatepetl.Infrastructure
- `Data/PopocatepetlDbContext.cs` — EF `DbContext` (SQLite)
- `Data/Configurations/` — `IEntityTypeConfiguration<T>` per entity
- `Data/Migrations/` — EF migrations
- `Data/Seeders/` — async idempotent seeders
- `Repositories/` — implementations of the `I*Repository` interfaces
- `DiffCalculator/`, `Email/`, `Export/`, `Services/` — adapter implementations
- `InfrastructureServiceExtensions.cs` — `AddInfrastructure(configuration)`

**Rule:** implements Domain interfaces. Never expose Infrastructure types upward — presentation should not see `PopocatepetlDbContext` or `ResendEmailService`.

### Popocatepetl.CLI and Popocatepetl.Api
- The CLI presents the interactive Spectre menu (`StackNavigator`, `MenuNode`, `IMenuAction`s).
- The API exposes thin REST controllers that delegate to MediatR.
- Both register their own DI extension (`AddCli()`, in the API see `Program.cs`).
- Both implement `ICurrentUserContext` differently — `CurrentUserContext` (CLI, session-based) and `HttpCurrentUserContext` (API, request-scoped).

**Rule:** presentation never calls a repository directly. Always go through `IMediator.Send(...)`.

## How a request flows

Walking through what happens when an admin picks "Download latest report" in the CLI:

1. `StackNavigator` calls `DownloadReportAction.ExecuteAsync(ct)`.
2. The action sends a MediatR request: `await _mediator.Send(new DownloadLatestArkReportCommand(), ct)`.
3. The MediatR pipeline runs. `AuditLoggingBehavior<TRequest,TResponse>` activates because the command implements `IAuditableRequest`. It will wrap the handler call in a try/catch and record success or failure to `audit_logs`.
4. `DownloadLatestArkReportCommandHandler.Handle(...)` runs. It:
   - Checks `currentUserContext.Role == UserRole.Admin` and returns `Result<…>.Failure("Only admins …")` if not.
   - Calls `IArkReportClient.DownloadLatestAsync(ct)` — Infrastructure adapter.
   - Reads previous latest via `IReportRepository.GetLatestAsync()`.
   - Calls `IDiffCalculator.Calculate(...)` if a baseline exists.
   - Saves through `IReportRepository.SaveAsync(...)` and `IDiffResultRepository.AddAsync(...)`.
   - Returns `Result<DownloadLatestArkReportResponse>.Success(...)`.
5. `AuditLoggingBehavior` writes a success row (with `ActionName` and `Detail` from the command).
6. Back in the action, the result is rendered to the user via `IAnsiConsole`.

The same shape applies to API requests — controllers do steps 1 and 6, MediatR does the rest.

## The Result&lt;T&gt; envelope

Defined at `Popocatepetl.Application/Common/Result.cs`. Three rules:

1. **Handlers return `Result<T>`** for any failure the caller might reasonably want to display.
   ```csharp
   return Result<…>.Failure("Only admins can download reports.");
   return Result<…>.Success(response);
   ```
2. **Throw exceptions only for genuinely unexpected failures** (programming bugs, framework crashes). The audit behavior catches them and records a failure row before re-throwing.
3. **Never use both for the same condition.** Pick `Result<T>` for "the user did something the rules don't allow" and exceptions for "this should never happen".

See the [[error-handling]] skill for the full split.

## Current-user identity

`ICurrentUserContext` (`Popocatepetl.Application/Common/ICurrentUserContext.cs`) exposes `Email` and `Role`. It is the only legitimate source of "who is doing this".

- The CLI registers `CurrentUserContext` as a singleton populated by `EmailLoginPrompt`.
- The API registers `HttpCurrentUserContext` as scoped, backed by `HttpContext`.
- Handlers **must** authorize against this — never trust a `UserId` field on an incoming request.

## DI conventions

Each layer ships its own extension method:

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddCli();
```

Compose them in this order in `Program.cs`. Don't register handlers, repositories, or pipeline behaviors anywhere else.

## Anti-patterns

- `using Popocatepetl.Infrastructure;` in an Application file. The interface lives in Domain — depend on that.
- Calling a repository directly from a menu action or controller. Always go through MediatR.
- Throwing for "user not allowed" or "input missing". Return `Result.Failure` so the audit behavior records it cleanly.
- Adding `[EntityFrameworkAttribute]` to a Domain entity. Configure persistence in `Infrastructure/Data/Configurations/`.
- Renaming the `Async` suffix off methods, or dropping `CancellationToken` parameters.
- Mocking `Result<T>` or domain entities in tests. They have no external dependencies — use the real types.

## Related skills

- [[add-feature]] for end-to-end checklists when adding a new use case.
- [[error-handling]] for the Result/exception split.
- [[audit-logging]] for what gets logged automatically.
- [[testing]] for how to test through this architecture.
- [[database]] for repositories, migrations, and entity configurations.
