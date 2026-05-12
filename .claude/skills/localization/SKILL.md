---
name: localization
description: Documents the CLI's i18n setup - CliStrings.*.resx resources, IStringLocalizer injection, LocaleStore and LocaleState, key naming, and how to add a new language. Use when adding/changing any user-facing string in the CLI, or wiring up a new language.
argument-hint: "[key name, language tag, or 'add language X']"
---

# Localization

Every CLI string a user sees is localized. Inline English in `Popocatepetl.CLI/Menus/...` is a bug. The setup is small but has a few rules that aren't visible from a single file.

## Layout

```
Popocatepetl.CLI/
└── Localization/
    ├── CliStrings.cs         ← marker class, IStringLocalizer<CliStrings>
    ├── CliStrings.resx       ← default (English) resources
    ├── CliStrings.cs.resx    ← Czech translations
    ├── CliStrings.sk.resx    ← Slovak translations
    ├── LocaleState.cs        ← in-memory current culture, mutable singleton
    └── LocaleStore.cs        ← persistent setting (reads/writes AppSetting)
```

- `CliStrings.cs` is a near-empty class that exists only so `IStringLocalizer<CliStrings>` has a stable type to resolve from.
- The `CliStrings.resx` *without* a culture tag is the **invariant culture**, treated as English.
- `LocaleStore.DefaultLocale` is `"en"`, and `LocaleStore.Supported` lists `"en"`, `"cs"`, `"sk"`. Both live in `LocaleStore.cs` — update them together when adding a language.

## Reading a string

Inject the localizer where needed:

```csharp
public sealed class SomeAction(IStringLocalizer<CliStrings> loc, IAnsiConsole console)
    : IMenuAction
{
    public string LabelKey => "menu.admin.export-diff";

    public Task ExecuteAsync(CancellationToken ct)
    {
        console.MarkupLine(loc["info.export.started"].Value);
        return Task.CompletedTask;
    }
}
```

Indexing the localizer returns a `LocalizedString` — call `.Value` for the actual text. Format-string lookups support positional placeholders too: `loc["info.deleted-n", count].Value`.

## Key naming conventions

Keys are dot-separated, kebab-case inside segments.

- `menu.<role>.<action>` for menu item labels: `menu.admin.export-diff`, `menu.power-user.send-report`, `menu.exit`, `menu.back`.
- `prompt.<purpose>` for prompts the user is asked: `prompt.continue`, `prompt.email-recipients`, `prompt.password`.
- `info.<topic>` for status messages: `info.report-downloaded`, `info.diff-recomputed`.
- `error.<topic>` for user-visible errors: `error.invalid-email`, `error.not-admin`.
- `role.<value>` for role labels shown to the user: `role.admin`, `role.user`, `role.power-user`.

**Stable keys are sacred.** Renaming a key without updating all three `.resx` files (and every `.Value` reference) silently falls back to the key string. Renames need a global find-replace across the CLI project + all three resource files.

## Adding a key

1. Open all three resx files (`CliStrings.resx`, `CliStrings.cs.resx`, `CliStrings.sk.resx`).
2. Add the same `<data name="...">` entry to each one. Keep them sorted alphabetically — easier to spot duplicates and misses.
3. If you don't have a translation ready, copy the English value into the other files temporarily, but **track it** (commit message or TODO). Shipping the English string in the Czech resx is loudly wrong; shipping a missing key is silently wrong, so visible is better.

## Localizing labels vs. localizing actions

Menu items have a `LabelKey`, not an inline label:

```csharp
public string LabelKey => "menu.admin.export-diff";
```

`StackNavigator` and `MenuChrome` resolve the key through the localizer before displaying. **Do not pre-resolve in the action.** The same `LabelKey` is what the audit log filter and tests rely on, so resolving early breaks both.

## How the current culture is set

Boot order in `Popocatepetl.CLI/Program.cs`:

```csharp
var locale = await sp.GetRequiredService<LocaleStore>().GetActiveAsync(cts.Token);
sp.GetRequiredService<LocaleState>().Set(CultureInfo.GetCultureInfo(locale));
```

1. `LocaleStore` reads `AppSettings` row for `AppSettingType.Language`. Falls back to `"en"` if the stored tag isn't in `Supported`.
2. `LocaleState.Set(culture)` updates an in-memory `CultureInfo` *and* the four `CultureInfo.*Culture` statics. From that point on, `IStringLocalizer<CliStrings>` resolves against the chosen culture.

`StackNavigator.RunAsync` calls `LocaleState.AlignCurrentThread()` between menu steps so culture survives even when Spectre spins up auxiliary threads.

## Changing the language at runtime

The `LanguageMenuAction` in `Popocatepetl.CLI/Menus/Settings/` lets a user pick a new language. The flow is:

1. Read `LocaleStore.Supported`, ask the user to pick.
2. Call `LocaleStore.SetActiveAsync(culture)` → writes the new tag into `AppSettings`.
3. Call `LocaleState.Set(culture)` → switches the in-process culture so subsequent strings resolve to the new language without a restart.

Both steps are required. Persisting without updating `LocaleState` leaves the running session in the old language; switching `LocaleState` without persisting reverts on the next launch.

## Adding a new language — checklist

To add, say, German (`de`):

- [ ] Create `Popocatepetl.CLI/Localization/CliStrings.de.resx` with every key from the English resx, translated
- [ ] Add `"de"` to `LocaleStore.Supported`
- [ ] If your `LanguageMenuAction` shows display names for each culture, add the German entry to its list/resource
- [ ] Test that the menu shows the language and switching works end-to-end
- [ ] Ensure the keys list has no missing entries — a script that diffs key sets across resx files is worth running before merging

## Anti-patterns

- **Inline English strings in CLI code.** Even for "debug" output — Spectre eats colors and people see your literal string. Use a key.
- **Concatenated strings for plurals.** `loc["info.deleted"].Value + " " + count + " rows"` produces grammatically wrong output in inflected languages. Use a parameterized key: `loc["info.deleted-n", count]` and let translators handle plural forms.
- **Storing display names in the database.** Anything user-facing should resolve through the localizer at render time, not be snapshotted.
- **Hardcoding the culture tag.** Read it from `LocaleState.Current` if you need it.
- **Skipping the resx update for non-English files.** A missing key falls back to the key string itself, which is ugly and unlocalized.

## Related skills

- [[add-feature]] — every new menu action needs all three resx files updated
- [[clean-architecture]] — localization is a CLI presentation concern; handlers don't touch `IStringLocalizer`
- [[comments]] — comments are not user-facing and don't get localized
