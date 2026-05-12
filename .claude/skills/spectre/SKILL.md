---
name: spectre
description: Documents Popocatepetl's Spectre.Console UI layer - the prompt abstractions (ITextPrompt / ISelectPrompt / IConfirmPrompt), their three implementations (Spectre / Stub / Native), the MenuNode tree, the StackNavigator runtime, MenuChrome, theming. Use when adding menu actions, building prompts, or debugging CLI UI flow.
argument-hint: "[component, prompt type, or menu question]"
---

# Spectre & Menu System

The CLI presentation is built on **Spectre.Console**, but the code never calls Spectre directly from menu actions. Three thin abstractions sit in front of it (`ITextPrompt`, `ISelectPrompt`, `IConfirmPrompt`) so tests, piped input, and the real terminal share one code path. The menu itself is a tree of `MenuNode` records walked by `StackNavigator`.

## Prompt abstractions

Defined in `Popocatepetl.CLI/Prompts/`:

```csharp
// Text input with optional validation and a "secret" mode for passwords.
public interface ITextPrompt
{
    Task<string> AskAsync(
        string labelKey,
        Func<string, ValidationResult>? validate = null,
        bool secret = false,
        CancellationToken ct = default);
}

// One-of-N pick. Title and labels are i18n keys; choices carry the typed value.
public interface ISelectPrompt
{
    Task<T> AskAsync<T>(
        string titleKey,
        IReadOnlyList<SelectChoice<T>> choices,
        CancellationToken ct = default) where T : notnull;
}

public sealed record SelectChoice<T>(T Value, string LabelKey, bool IsCurrent = false);

// Yes/no with a default.
public interface IConfirmPrompt
{
    Task<bool> AskAsync(string labelKey, bool defaultValue = false, CancellationToken ct = default);
}
```

Three notes that matter:

- `labelKey` / `titleKey` are **localization keys**, not literal strings. They are resolved by the prompt implementation against `IStringLocalizer<CliStrings>`. See [[localization]].
- `SelectChoice<T>.IsCurrent` is for "this is the currently active option" — used by the theme menu, language menu, etc. The Spectre implementation highlights it; the Stub ignores it.
- `ValidationResult` carries `ErrorKey` (a localization key), not a raw message. Validators return localized errors:

  ```csharp
  return string.IsNullOrWhiteSpace(input)
      ? ValidationResult.Error("error.email-required")
      : ValidationResult.Ok();
  ```

## Three implementations

| Folder | When it's wired | What it does |
| --- | --- | --- |
| `Prompts/Spectre/` | Default — real terminal | Calls `AnsiConsole.Prompt(...)`. Uses the active theme palette for colors. |
| `Prompts/Stub/` | `Console.IsInputRedirected` is true (piped stdin, redirected files, **tests**) | Returns canned answers or empties without blocking on input. |
| `Prompts/Native/` | Always — for OS file dialogs | `NativeFileSaveDialog` only. Different concern from the three prompt interfaces, lives here because it's still a prompt-like interaction. |

The switch is in `Popocatepetl.CLI/DependencyInjection.cs`:

```csharp
if (Console.IsInputRedirected)
{
    services.AddSingleton<ITextPrompt, StubTextPrompt>();
    services.AddSingleton<ISelectPrompt, StubSelectPrompt>();
    services.AddSingleton<IConfirmPrompt, StubConfirmPrompt>();
}
else
{
    services.AddSingleton<ITextPrompt, SpectreTextPrompt>();
    services.AddSingleton<ISelectPrompt, SpectreSelectPrompt>();
    services.AddSingleton<IConfirmPrompt, SpectreConfirmPrompt>();
}
```

**Why this matters for actions.** A menu action that calls `_console.Input.ReadKey(...)` directly will hang in `dotnet test` (stdin is redirected, Spectre's read blocks forever). Always go through the abstractions, or check `Console.IsInputRedirected` and bail out. `MenuChrome.WaitForContinue()` already does this guard — copy the pattern.

## MenuNode — the tree

`Popocatepetl.CLI/Navigation/MenuNode.cs` is a record with three factories:

```csharp
MenuNode.Branch(labelKey, requiredRole, params children);
MenuNode.GuardedBranch(labelKey, requiredRole, guard, params children);
MenuNode.Leaf(labelKey, requiredRole, action);
```

Fields:

- **`LabelKey`** — localized label shown in the parent's select prompt and as the title when this node is entered.
- **`RequiredRole`** — the minimum role to *see* this node. `StackNavigator` filters children by `session.Role >= node.RequiredRole`. The menu hides nodes the current user lacks the role for; the handler must still enforce the same check (see [[clean-architecture]]).
- **`Action`** — non-null on leaves only. `StackNavigator` invokes it when the leaf is picked.
- **`EntryGuard`** — async predicate that runs **before** the branch is pushed onto the stack. If it returns false, the branch is silently skipped and the parent menu re-prompts. Use it for things like "admin password gate before opening admin submenu".
- **`Children`** — never null. Empty for leaves, populated for branches.

### Composing the tree

`Popocatepetl.CLI/Menus/TopMenuFactory.cs` builds the root:

```csharp
public MenuNode Build()
{
    return MenuNode.Branch("menu.top.title", UserRole.None,
        MenuNode.GuardedBranch("menu.admin.title", UserRole.None,
            ct => _adminGate.AskForPasswordAsync(ct),
            _adminMenuFactory.Build()),                  // admin submenu
        _powerUserMenuFactory.Build(),
        _userMenuFactory.Build(),
        _settingsMenuFactory.Build());
}
```

Each role-specific factory (`Menus/Admin/`, `Menus/PowerUser/`, `Menus/User/`, `Menus/Settings/`) returns a `MenuNode`. **Keep the factories thin** — they wire `MenuNode.Leaf(LabelKey, Role, action)` and don't do business logic.

### Role gating is not authorization

`RequiredRole` is a UX filter. A user without the role won't see the menu item, so they can't pick it through the keyboard. But the same MediatR command could still be sent from a test, a script, or a malicious caller. The handler **must** check `currentUserContext.Role` and return `Result.Failure` if it's wrong. See [[add-feature]] step 4 and [[clean-architecture]].

### Entry guards

Used for the admin password gate today. The signature is `Func<CancellationToken, Task<bool>>`. Two rules:

- The guard runs every time the branch is opened. Don't cache decisions for the rest of the session unless that's deliberate (the admin gate currently does grant a session-wide pass via `AdminPasswordGate.IsUnlocked` — see `Popocatepetl.CLI/Auth/AdminPasswordGate.cs`).
- A returned `false` is **silent**. If the user needs to know "wrong password", the guard should print that itself before returning false.

## StackNavigator — the runtime

`Popocatepetl.CLI/Navigation/StackNavigator.cs` walks the tree.

Each iteration:

1. Filter the current node's children by `session.Role`.
2. Add a synthetic `menu.back` (when not at the root) and `menu.exit` choice.
3. Show the choices via `ISelectPrompt`, titled with the node's `LabelKey`.
4. Resolve the picked child:
   - `menu.exit` → return out of the loop, the run ends.
   - `menu.back` → pop the current node off the stack.
   - **Leaf**: call `action.ExecuteAsync(ct)`, then loop again at the same level.
   - **Branch**: evaluate `EntryGuard` if present. If true (or absent), push and descend. If false, loop again at the same level.

After every iteration, `LocaleState.AlignCurrentThread()` is called so culture survives Spectre's auxiliary threads — see [[localization]].

### Common control-flow gotchas

- **Calling a leaf does not pop the parent.** The user stays at the parent menu until they explicitly pick `menu.back` or `menu.exit`. Don't introduce "auto-return" — it disorients users.
- **`menu.back` and `menu.exit` keys are reserved.** Don't reuse them for leaf labels.
- **Cancellation tokens propagate.** The `cts` set up in `Program.cs` is wired to Ctrl+C and forwards to the navigator, every prompt, and every action. Long-running actions should respect it; passing `ct` through MediatR sends it down to handlers.

## MenuChrome

`Popocatepetl.CLI/Navigation/MenuChrome.cs` renders the persistent top bar shown above every prompt:

```
─── Popocatepetl ─────────────────── user@example.com · admin ──
```

Two public methods:

- **`RenderTop()`** — call at the start of an action that wants to feel like a "page". Re-renders the rule and the user context. Cheap.
- **`WaitForContinue()`** — prints the localized `prompt.continue` and blocks on a key press. **Guarded against `Console.IsInputRedirected`** so it doesn't hang in tests. Use it at the end of leaf actions that printed output the user should read before the menu re-draws.

The chrome reads the **active palette** from `ThemeApplier.Active` so colors track whatever theme the Power User picked.

## Theming

Three pieces (`Popocatepetl.CLI/Theming/`):

- **`ThemeApplier`** — singleton holding the active palette. Action code reads `_theme.Active.Heading`, `.Muted`, `.BorderStyle()`, etc.
- **`ThemeStore`** — persists the active palette name in `AppSettings`, same way `LocaleStore` does for language.
- **Palettes** — value objects with named color slots (Heading, Body, Muted, Border, Highlight, Warn, Error).

Boot order in `Program.cs`:

```csharp
var palette = await sp.GetRequiredService<ThemeStore>().GetActiveAsync(cts.Token);
sp.GetRequiredService<ThemeApplier>().Apply(palette);
```

Changing the theme at runtime mirrors the language switch: persist via `ThemeStore.SetActiveAsync`, apply via `ThemeApplier.Apply(palette)` so subsequent prompts pick up the new colors immediately.

## Spectre output rules

- **Use the injected `IAnsiConsole`**, never `AnsiConsole.MarkupLine` directly. Tests inject `Spectre.Console.Testing.TestConsole` — going through the static API bypasses it.
- **Escape user-supplied text** with `Markup.Escape(...)` before interpolating into a markup string. An unescaped `[` in a user's email crashes Spectre's parser.
- **Use the theme palette for colors.** `console.MarkupLine($"[{palette.Muted}]{text}[/]")`. Don't hardcode `[grey]`/`[red]`. The palette ensures the theme is consistent.
- **Don't bypass MediatR for "just a small thing".** A menu action that reads or writes data via repositories directly skips the audit log, the role check, and the consistent error rendering. See [[clean-architecture]].

## Stub implementations & tests

The `Stub*` prompts are the test fixtures' bread and butter:

- **`StubTextPrompt`** — returns empty strings or a queue of preset responses, depending on what your test sets up.
- **`StubSelectPrompt`** — returns the first non-current choice, or whatever the test scripts.
- **`StubConfirmPrompt`** — returns the default value passed in.

For richer test flows, the dedicated `ScriptedSelectPrompt` in `Popocatepetl.CLI.Tests/TestUtilities/` plays back a queue of label keys (matching the user's keyboard picks) and records what was offered. Drive `StackNavigator.RunAsync(...)` with it and assert against `prompt.AskedTitleKeys` and `prompt.OfferedChoiceLabels`. See [[testing]] for the full pattern.

## Adding a new menu — checklist

- [ ] Three localization entries (`menu.<role>.<action>`) in all three `CliStrings.*.resx` files
- [ ] `IMenuAction` class in `Popocatepetl.CLI/Menus/<Role>/`
- [ ] DI registration in `Popocatepetl.CLI/DependencyInjection.cs` (scoped, alongside `ChangeThemeAction`, `SendReportAction`, etc.)
- [ ] Wired into the right factory under `Menus/<Role>/` via `MenuNode.Leaf(LabelKey, Role, action)`
- [ ] Flow test using `ScriptedSelectPrompt` + `TestConsole` if it changes navigation behavior
- [ ] Role check inside the handler the action invokes (the menu hides it; the handler still enforces it)

## Related skills

- [[clean-architecture]] — actions delegate to MediatR; they do not own logic
- [[add-feature]] — end-to-end checklist for new commands and the menu wiring they need
- [[localization]] — every label/title is a key, not literal English
- [[testing]] — `ScriptedSelectPrompt` and `TestConsole` patterns
