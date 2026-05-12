---
name: add-feature
description: Step-by-step checklist for adding a new use case to Popocatepetl - new command/query handler, new CLI menu action, and/or new API endpoint. Use when the user asks to add functionality that does not yet exist.
argument-hint: "[feature name or 'CLI action / API endpoint']"
---

# Adding a Feature

A new feature in this codebase usually means three things together: a MediatR request + handler in Application, optional adapter work in Infrastructure, and a presentation hookup in either CLI or API (sometimes both). Follow the steps in order — earlier ones unblock later ones.

## Decision tree

Before writing code, decide:

1. **State-changing or read-only?**
   - Changes data → `Command`
   - Reads data → `Query`

2. **Where will it be invoked from?**
   - Spectre menu → CLI menu action
   - REST → API controller endpoint
   - Both → write the handler once, expose from both layers

3. **Who is allowed to invoke it?**
   - `User`, `Admin`, `PowerUser`, or any role
   - Role checks live **inside the handler**, not just the menu — see step 4

4. **Does it need to appear in the audit log?**
   - User-visible action → yes, implement `IAuditableRequest` (see [[audit-logging]])
   - Background/system → no

## Application layer

### 1. Create the request record

`Popocatepetl.Application/Commands/{Area}/XxxCommand.cs` or `.../Queries/{Area}/XxxQuery.cs`.

```csharp
using MediatR;
using Popocatepetl.Application.Common;

namespace Popocatepetl.Application.Commands.Reports;

public sealed record ExportDiffCommand(Guid DiffResultId, DiffExportFormat Format)
    : IAuditableRequest, IRequest<Result<ExportDiffResponse>>
{
    public string ActionName => "ExportDiff";
    public string Detail => $"DiffResultId: {DiffResultId}, Format: {Format}";
}
```

Rules:
- Use a `record` (immutable, value equality).
- Return `Result<T>` when the operation can fail in a user-recoverable way; `Unit` when it cannot.
- Implement `IAuditableRequest` for user-visible operations and fill in `ActionName` (PascalCase verb) and `Detail` (parameters worth seeing in the log).

### 2. Create the response DTO (if returning data)

`Popocatepetl.Application/Dtos/ExportDiffResponse.cs`. Records preferred.

```csharp
public sealed record ExportDiffResponse(string FilePath, long FileSizeBytes);
```

### 3. Create the handler

`Popocatepetl.Application/Handlers/{Area}/XxxCommandHandler.cs`. Use primary constructor injection.

```csharp
public sealed class ExportDiffCommandHandler(
    IDiffResultRepository diffResultRepository,
    IDiffExporter diffExporter,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ExportDiffCommand, Result<ExportDiffResponse>>
{
    public async Task<Result<ExportDiffResponse>> Handle(
        ExportDiffCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUserContext.Role != UserRole.Admin)
            return Result<ExportDiffResponse>.Failure("Only admins can export diffs.");

        var diff = await diffResultRepository.GetByIdAsync(request.DiffResultId);
        if (diff is null)
            return Result<ExportDiffResponse>.Failure("Diff not found.");

        var output = await diffExporter.ExportAsync(diff, request.Format, cancellationToken);
        return Result<ExportDiffResponse>.Success(new(output.Path, output.SizeBytes));
    }
}
```

Rules:
- Depend on Domain interfaces (`I*Repository`, `I*Service`, etc.) — never on concrete Infrastructure types.
- Authorize against `currentUserContext.Role` and return a failure result instead of throwing.
- Keep the handler thin. Multi-step orchestration is fine; algorithmic work belongs behind a Domain port.

### 4. Authorization is in the handler

The menu may hide an action from a `User`, but a malicious caller could still send the same MediatR request through code. The handler is the last gate. Check `currentUserContext.Role` and return `Result.Failure` for unauthorized callers.

## Infrastructure layer — only if you need a new adapter

If the handler needs a new I/O capability (HTTP, file, third-party SDK), do **not** add it inline:

1. Define an interface in `Popocatepetl.Domain/Interfaces/IXxx.cs`.
2. Implement it in `Popocatepetl.Infrastructure/<area>/XxxImplementation.cs`.
3. Register it in `Popocatepetl.Infrastructure/InfrastructureServiceExtensions.cs` — pick lifetime carefully (`AddScoped` for anything touching the DB, `AddSingleton` for stateless adapters, `AddHttpClient<>` for HTTP).

If the new feature needs a new persistent entity, follow the [[database]] skill for entity + configuration + migration + repository.

## Presentation layer

### Option A — CLI menu action

1. **Add a localized label** for the menu item in all three `Popocatepetl.CLI/Localization/CliStrings.*.resx` files. Use a stable key like `menu.admin.export-diff`. See [[localization]].

2. **Create the action** in `Popocatepetl.CLI/Menus/<Role>/XxxAction.cs`:

   ```csharp
   public sealed class ExportDiffAction(
       IMediator mediator,
       IAnsiConsole console,
       IStringLocalizer<CliStrings> loc,
       MenuChrome chrome)
       : IMenuAction
   {
       public string LabelKey => "menu.admin.export-diff";

       public async Task ExecuteAsync(CancellationToken ct)
       {
           chrome.RenderTop();
           var result = await mediator.Send(new ExportDiffCommand(...), ct);
           if (result.IsSuccess)
               console.MarkupLine(...);
           else
               console.MarkupLine(...);
           chrome.WaitForContinue();
       }
   }
   ```

3. **Register the action** as `Scoped` in `Popocatepetl.CLI/DependencyInjection.cs` next to the existing `ChangeThemeAction`, `SendReportAction`, etc.

4. **Wire it into the menu tree** in `Popocatepetl.CLI/Menus/TopMenuFactory.cs` (or its submenu factories under `Menus/Admin/`, `Menus/PowerUser/`, `Menus/User/`). Use `MenuNode.Leaf(LabelKey, Role, action)` and pick the lowest role allowed to **see** the option. The handler still enforces the real check.

### Option B — API endpoint

1. **Add (or pick) a controller** in `Popocatepetl.Api/Controllers/`.
2. **Add a request DTO** in `Popocatepetl.Api/Dtos/` if the wire shape differs from the command.
3. **Action method** — thin, MediatR-driven:

   ```csharp
   [HttpPost("export")]
   public async Task<ActionResult<ExportDiffResponse>> Export(
       [FromBody] ExportDiffRequest request,
       CancellationToken ct)
   {
       var result = await _mediator.Send(
           new ExportDiffCommand(request.DiffResultId, request.Format), ct);

       return result.IsSuccess
           ? Ok(result.Value)
           : BadRequest(result.ErrorMessage);
   }
   ```

4. **Do not authorize in the controller.** `HttpCurrentUserContext` is wired so the handler sees the current user.

## Tests

For every new handler:

1. **Handler unit test** in `Popocatepetl.Application.Tests/Handlers/{Area}/XxxCommandHandlerTests.cs`. Extend `HandlerTestBase` if your handler needs the standard mocked services; otherwise use `MockedIocBuilder` directly to register only what you need. Cover at least:
   - Happy path
   - Unauthorized caller → `Result.Failure`
   - Repository-not-found → `Result.Failure`
   - Audit row written on success and on failure

2. **CLI flow test** (if you added a menu action) in `Popocatepetl.CLI.Tests/Menus/...`. Drive it through `ScriptedSelectPrompt` + `TestConsole`. Assert which `LabelKey`s were offered, which sub-flows were entered, and that the action was called.

See the [[testing]] skill for the full toolbox and patterns.

## Quick checklist

- [ ] Command/Query record in `Application/Commands` or `Application/Queries`
- [ ] Handler in `Application/Handlers/{Area}`
- [ ] Implements `IAuditableRequest` if user-visible
- [ ] Returns `Result<T>` (not throw) for expected failures
- [ ] Role check inside the handler
- [ ] New interface in `Domain/Interfaces` if a new external dependency
- [ ] Implementation in `Infrastructure` and registered in `InfrastructureServiceExtensions`
- [ ] CLI: action class, DI registration, menu tree wiring, **all three resx files** updated
- [ ] API: controller endpoint, DTOs in `Popocatepetl.Api/Dtos`
- [ ] Handler tests cover happy path, auth failure, validation failure
- [ ] CLI flow tests if a menu action was added
- [ ] `dotnet build` clean, `dotnet test` green

## Related skills

- [[clean-architecture]] — layering rules
- [[audit-logging]] — `IAuditableRequest` details
- [[localization]] — adding `resx` keys
- [[testing]] — patterns and libraries
- [[database]] — when the feature needs schema changes
