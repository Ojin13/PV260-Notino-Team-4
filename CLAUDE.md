# Popocatepetl — agent guide

> Read this before making any change. It is the source of truth for architecture, conventions, and where things live.

## What this project is

Popocatepetl is a **Windows console application** (with a thin REST companion API) that:

- Downloads the latest **ARK ETF holdings report** from ark-funds.com (admin-triggered).
- **Diffs** it against the previously stored report.
- Lets users **browse**, **export**, and **email** the diff.
- Logs **every action** to an audit trail.

It supports three roles — **User**, **Admin** (password-gated), **Power User** — plus switchable color themes and a translated UI (i18n).

> The user stories describe reports generically; the implementation is ARK-specific (`IArkReportClient` / `ArkReportClient`). The user-facing presentation site copy stays generic; internal code does not need to.

**Important: only the latest report is retained.** There is no historical archive. A new download replaces the "latest" pointer and produces one new diff against the previous latest. User stories US-06 (diff history) and US-07 (per-company history) are **not implemented**; do not write code or copy that implies they are.

---

## Architecture — Clean Architecture, four layers

```
┌────────────────────────────────────────────────────────────────┐
│  Presentation                                                  │
│    Popocatepetl.CLI    Popocatepetl.Api                        │
│    (Spectre menu)      (REST controllers — minimal)            │
└──────────────────┬─────────────────────────────────────────────┘
                   │ depends on
                   ▼
┌────────────────────────────────────────────────────────────────┐
│  Popocatepetl.Infrastructure                                   │
│    EF Core (SQLite) · Resend email · ArkReportClient (HTTP)    │
│    QuestPDF export · Repositories implement Domain interfaces  │
└──────────────────┬─────────────────────────────────────────────┘
                   │ depends on
                   ▼
┌────────────────────────────────────────────────────────────────┐
│  Popocatepetl.Application                                      │
│    MediatR commands/queries + handlers                         │
│    AuditLoggingBehavior (cross-cutting)                        │
│    Result<T> envelope, ICurrentUserContext, IAuditableRequest  │
└──────────────────┬─────────────────────────────────────────────┘
                   │ depends on
                   ▼
┌────────────────────────────────────────────────────────────────┐
│  Popocatepetl.Domain                                           │
│    Entities, Enums, Value Objects, Interfaces (ports), Events  │
│    No outward dependencies. No EF, no HTTP, no Spectre.        │
└────────────────────────────────────────────────────────────────┘
```

**Dependency direction is strict.** A lower layer never references a higher one. Domain interfaces (`IArkReportClient`, `IEmailService`, `IDiffExporter`, `I*Repository`, etc.) are the ports; Infrastructure provides the adapters.

---

## Request flow — how a click reaches the database

1. **User picks a menu item** in the CLI. The selected `IMenuAction` is invoked by `StackNavigator`.
2. The action **sends a MediatR `IRequest`** (`Command` or `Query`) via `IMediator.Send(...)`.
3. **MediatR pipeline behaviors** run first. Right now there is one: `AuditLoggingBehavior` — it runs for any request that implements `IAuditableRequest` and writes a success/failure entry to the `audit_logs` table around the handler.
4. The **handler** in `Popocatepetl.Application/Handlers/...` resolves repositories/services through their domain interfaces, performs the work, and returns a `Result<T>` (success/failure envelope — do not throw for expected failures).
5. The handler **never references EF directly.** It uses repositories defined in `Popocatepetl.Domain/Interfaces/` and implemented in `Popocatepetl.Infrastructure/Repositories/`.
6. The action **renders the `Result<T>`** back through Spectre (or returns an HTTP response in the API).

The same flow applies to the API (`Popocatepetl.Api/Controllers/*Controller.cs`) — controllers are thin and delegate to MediatR.

---

## Folder map — where to look

### Solution root
```
Popocatepetl.sln              ← solution
appsettings.json              ← lives inside Popocatepetl.CLI/ and Popocatepetl.Api/
.github/workflows/release.yml ← Windows self-contained build on push to main
```

### `Popocatepetl.Domain/`  — pure model, no IO
- `Entities/`        — `Report`, `DiffResult`, `DiffData`, `AppUser`, `AuditLog`, `AppSetting`, `MailAttachment`, `BaseEntity`
- `Enums/`           — `UserRole`, `AppSettingType`, `ShareDiffType`
- `Interfaces/`      — ports for everything Infrastructure provides (`IArkReportClient`, `IEmailService`, `IDiffCalculator`, `IDiffExporter`, `ICsvParser`, and `I*Repository`)
- `ValueObjects/`, `Events/`, `Exceptions/`

### `Popocatepetl.Application/`  — use cases via MediatR
- `Commands/Reports/`        — request DTOs for state-changing operations
- `Queries/{Admin,Users,UserRole}/` — request DTOs for reads
- `Handlers/{Reports,Admin,Users,UserRole}/` — `IRequestHandler<TReq,TRes>` implementations
- `Behaviors/AuditLoggingBehavior.cs` — pipeline cross-cut for audit
- `Common/`                  — `Result<T>`, `IAuditableRequest`, `ICurrentUserContext`, `DiffExportFormat`
- `Dtos/`                    — request/response shapes shared between handlers and presentation
- `DependencyInjection.cs`   — `AddApplication()` registers MediatR + behaviors

### `Popocatepetl.Infrastructure/`  — adapters
- `Data/PopocatepetlDbContext.cs` — EF `DbContext` (SQLite)
- `Data/Configurations/`     — `IEntityTypeConfiguration<T>` per entity
- `Data/Migrations/`         — EF migrations; **add new ones, never edit existing**
- `Data/Seeders/`            — async seeders called from `UseSeeding` / `UseAsyncSeeding`
- `Repositories/`            — implementations of the `I*Repository` interfaces
- `DiffCalculator/`          — `DiffCalculator` implementing `IDiffCalculator`
- `Email/`                   — `ResendEmailService` (Resend.com, not SendGrid)
- `Export/`                  — `DiffExporter` (QuestPDF for PDF, plus CSV)
- `Services/`                — `ArkReportClient`, options classes (`ArkReportOptions`, etc.)
- `InfrastructureServiceExtensions.cs` — `AddInfrastructure(configuration)`

### `Popocatepetl.CLI/`  — Spectre.Console presentation
- `Program.cs`               — host bootstrap; runs migrations, applies theme + locale, runs login, runs root menu
- `DependencyInjection.cs`   — `AddCli()` — note the **stub vs real prompt switch** based on `Console.IsInputRedirected`
- `Auth/AdminPasswordGate.cs` — guards the admin sub-menu
- `Menus/`                   — `TopMenuFactory.Build()` produces the `MenuNode` tree consumed by `StackNavigator`. Submenus per role under `Menus/{Admin,PowerUser,User,Settings}/`
- `Navigation/`              — `StackNavigator`, `MenuNode`, `MenuChrome`, `LocaleState`
- `Prompts/`                 — `ITextPrompt` / `ISelectPrompt` / `IConfirmPrompt` abstractions with three impls: `Spectre/`, `Native/`, `Stub/` (tests + redirected stdin)
- `Theming/`                 — `ThemeApplier`, `ThemeStore`, color palettes
- `Localization/`            — `CliStrings.{en,sk,cs}.resx`, `LocaleStore`, `LocaleState`
- `Services/CurrentUserContext.cs` — concrete `ICurrentUserContext` for the CLI session
- `Resources/`               — localized strings, palettes

### `Popocatepetl.Api/`  — REST companion (not user-facing presentation)
- `Controllers/`             — `AdminReportsController`, `AuditLogsController`, `EmailController`, `UsersController`, `UserRoleController`
- `Services/HttpCurrentUserContext.cs` — `ICurrentUserContext` impl backed by `HttpContext`
- `Validation/EmailListAttribute.cs` — model-binding validation for recipient lists
- `Dtos/`, `Program.cs`

### Tests
- `Popocatepetl.Application.Tests/Handlers/...` — handler tests using `TestUtilities/` fakes
- `Popocatepetl.Infrastructure.Tests/`          — repository + export tests, SQLite in-memory or file
- `Popocatepetl.CLI.Tests/{Auth,Menus,Navigation,Persistence}/` — UI flow tests using `Stub*` prompts and `Spectre.Console.Testing.TestConsole`
- Each project has `TestUtilities/` for shared fakes and a `GlobalUsings.cs` (xUnit, FluentAssertions, Moq already imported globally — do not re-import)

---

## Conventions — must-follow rules

### Layering
- **Domain has zero outward dependencies.** Adding `using Microsoft.EntityFrameworkCore;` to a Domain file is wrong.
- **Application depends on Domain only.** Anything Infrastructure-flavored (HTTP, EF, file IO) belongs behind a `Domain/Interfaces/I*` port.
- **Infrastructure implements Domain interfaces.** Never reference an Infrastructure type from Application.
- **Presentation does not call repositories directly.** Always go through MediatR.

### MediatR
- One request = one handler. File names: `XxxCommand.cs` + `XxxCommandHandler.cs`, or `XxxQuery.cs` + `XxxQueryHandler.cs`.
- Handlers return `Result<T>` (`Popocatepetl.Application/Common/Result.cs`). **Reserve exceptions for genuinely unexpected failures.**
- If an action should be in the audit log, the request **must implement `IAuditableRequest`** (provides `ActionName` and `Detail`). The `AuditLoggingBehavior` does the rest — do not manually call `IAuditLogRepository` from handlers.

### Role / authorization
- The current user is available via `ICurrentUserContext` (`Email`, `Role`). The CLI sets this through `EmailLoginPrompt` + `AdminPasswordGate`; the API sets it via `HttpCurrentUserContext`.
- Authorization is **inside the handler** — check `currentUserContext.Role` and return a `Result.Failure` when the caller is not allowed. Do not rely on the menu hiding an action; the handler must still refuse.

### Persistence
- Repositories live in `Infrastructure/Repositories/`. Each implements an `I*Repository` from `Domain/Interfaces/`.
- **Never edit an existing EF migration.** Add a new one with `dotnet ef migrations add <Name> -p Popocatepetl.Infrastructure -s Popocatepetl.CLI` and update `PopocatepetlDbContextModelSnapshot`.
- Seeders are wired into `InfrastructureServiceExtensions` via `UseSeeding` / `UseAsyncSeeding`; idempotent (re-running must not duplicate).

### Email
- The configured provider is **Resend** (`ResendEmailService`), not SendGrid — US-16 mentions SendGrid but the code does not use it.
- `IEmailService.SendAsync(...)` takes a single optional `MailAttachment? attachment = null`. **Emails carry at most one attachment** (see `feedback_email_single_attachment` memory).

### Logging / output
- CLI text output is exclusively through `IAnsiConsole`. Do not use `Console.WriteLine` in handlers or infrastructure (use Spectre at the edge).
- Localized strings come from `IStringLocalizer<CliStrings>`; new keys belong in `CliStrings.{en,sk,cs}.resx`. Never inline user-facing English in CLI code.

### Naming
- No `using Foo = Some.Fully.Qualified.Name;` aliases. Either fully-qualify inline or, when it's a project-wide type clash, rename. See [[feedback_no_type_aliases]] memory.
- Records over classes for immutable DTOs. `Result<T>` is a class today; do not change it without a wider discussion.
- Async methods end in `Async` and accept a trailing `CancellationToken ct` or `cancellationToken`.

### Tests
- xUnit + FluentAssertions + Moq. No Shouldly, no NSubstitute.
- Tests mirror the structure of the project under test.
- For CLI flows, prefer the `Stub*Prompt` family and `TestConsole` over driving Spectre directly.
- Run with `dotnet test`. No tests are skipped in CI.

---

## Important files — quick navigation

| Concern                          | File                                                                                                  |
| -------------------------------- | ----------------------------------------------------------------------------------------------------- |
| CLI entry point + boot order     | `Popocatepetl.CLI/Program.cs`                                                                         |
| CLI DI + prompt-impl switch      | `Popocatepetl.CLI/DependencyInjection.cs`                                                             |
| Root menu definition             | `Popocatepetl.CLI/Menus/TopMenuFactory.cs`                                                            |
| Menu tree primitive              | `Popocatepetl.CLI/Navigation/MenuNode.cs`, `StackNavigator.cs`                                        |
| Login & password gate            | `Popocatepetl.CLI/Menus/EmailLoginPrompt.cs`, `Popocatepetl.CLI/Auth/AdminPasswordGate.cs`            |
| Application DI + MediatR config  | `Popocatepetl.Application/DependencyInjection.cs`                                                     |
| Audit logging behavior           | `Popocatepetl.Application/Behaviors/AuditLoggingBehavior.cs`                                          |
| Result envelope                  | `Popocatepetl.Application/Common/Result.cs`                                                           |
| Audit marker interface           | `Popocatepetl.Application/Common/IAuditableRequest.cs`                                                |
| Current-user abstraction         | `Popocatepetl.Application/Common/ICurrentUserContext.cs`                                              |
| Infrastructure DI + DB/HTTP wire | `Popocatepetl.Infrastructure/InfrastructureServiceExtensions.cs`                                      |
| EF DbContext                     | `Popocatepetl.Infrastructure/Data/PopocatepetlDbContext.cs`                                           |
| ARK fetcher                      | `Popocatepetl.Infrastructure/Services/ArkReportClient.cs`                                             |
| Email provider                   | `Popocatepetl.Infrastructure/Email/ResendEmailService.cs`                                             |
| Diff calculator                  | `Popocatepetl.Infrastructure/DiffCalculator/DiffCalculator.cs`                                        |
| Diff export                      | `Popocatepetl.Infrastructure/Export/DiffExporter.cs`                                                  |
| Domain ports                     | `Popocatepetl.Domain/Interfaces/*.cs`                                                                 |
| Release pipeline                 | `.github/workflows/release.yml` (on `main`)                                                           |
| Pages site (separate branch)     | `github-page` branch — Nuxt 3 + workflow at `.github/workflows/deploy-pages.yml`                      |

---

## Common tasks — where things go

| Task                                     | Where to add code                                                                                                                                                        |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| New menu action                          | New `IMenuAction` in `Popocatepetl.CLI/Menus/<Role>/`, registered in `DependencyInjection.cs`, wired into `TopMenuFactory`                                                |
| New use case (state-changing)            | `Application/Commands/<Area>/XxxCommand.cs` + `Application/Handlers/<Area>/XxxCommandHandler.cs`. Implement `IAuditableRequest` if user-visible.                          |
| New use case (read)                      | Same layout under `Queries/` and `Handlers/<Area>/`                                                                                                                      |
| New entity                               | `Domain/Entities/Xxx.cs` + `Infrastructure/Data/Configurations/XxxConfiguration.cs` + EF migration + (often) `IXxxRepository` port and implementation                    |
| New external dependency (HTTP, file, …)  | Define `IXxx` in `Domain/Interfaces/`, implement in `Infrastructure/<area>/`, register in `InfrastructureServiceExtensions`                                               |
| New translatable string                  | Add the key to all three `Popocatepetl.CLI/Resources/CliStrings.*.resx`, then reference via `IStringLocalizer<CliStrings>["the.key"]`                                    |
| New configuration value                  | Add to `Popocatepetl.CLI/appsettings.json` (and `Popocatepetl.Api/appsettings.json` if relevant), bind via `IOptions<T>` in `InfrastructureServiceExtensions`             |

---

## Things that are intentionally not here

- **No data warehouse / report history.** Only the latest report is stored — see the project memory for the full reasoning.
- **No SendGrid.** Resend is the email provider; `SendGrid` strings in user stories are out of date.
- **No CLI subcommands.** The app is interactive only — entry is `Popocatepetl.CLI.exe` with no arguments. There is no `popocatepetl <verb>` interface.
- **No background scheduler.** Downloads are admin-triggered; if you want a daily run, that's a Windows scheduled task on the user's side, not application code.
