---
name: comments
description: Rules for writing comments in the Popocatepetl codebase. Comments must look human-written, not AI-generated. Default is no comment at all - only add one when the WHY is not obvious. Use when adding, editing, or reviewing any comment.
argument-hint: "[file or area you are commenting on]"
---

# Writing Comments

Comments in this codebase must not look like an AI wrote them. Keep them plain, short, human. **Default is no comment at all.** Only write one when the WHY is not obvious from the code.

If a rule below conflicts with something you see in the codebase, fix the codebase to match the rules, do not loosen the rules.

## Hard rules

1. **Always use `//`.** Never `///`, `<summary>`, `<see cref>`, `<c>`, `<para>`, `<list>`, `<inheritdoc/>`, `<param>`, `<returns>`, or any other XML doc markup. When you touch a file that still has XML comments, convert them in the same pass.

2. **Never use an em-dash (`—`) or en-dash (`–`) inside comments.** Use a normal hyphen `-` or rewrite the sentence without a dash. (Em-dashes are fine in user-facing copy outside the C# codebase — site copy, README — but not inside `//` comments.)

3. **No decorated references to variables, methods, or types.** Don't wrap names in `[brackets]`, backticks, or special quotes. Write the name as a plain word.
   - Correct: `Nulled when the template is deleted.`
   - Wrong: `Nulled when the [template] is deleted.`
   - Wrong: ``Nulled when the `template` is deleted.``

4. **Keep comments short.** 2 to 3 lines max. One line is usually enough. If the explanation doesn't fit in three lines, the code probably needs to change, not the comment.

5. **No fancy formatting.** Plain English sentences. No bullet lists, no `**bold**`, no markdown headings, no ASCII boxes, no code snippets inside comments.

6. **Simple English, around C1 level.** Short words, short sentences. Contractions like `it's`, `doesn't`, `don't` are welcome. Avoid rare or technical-sounding vocabulary when a plain word works.

7. **Current state only.** Comments describe how the code works *right now*. Never mention legacy code, an original solution, a previous approach, phases, or steps from any implementation plan. No "was X, now Y". No "phase 1". No `TODO remove once Y ships` unless the user explicitly asked for a TODO.

8. **Avoid telltale AI phrases.** Don't start a comment with:
   - "This method handles..."
   - "This class represents..."
   - "Note that..."
   - "It is important to note that..."
   - "Please ensure..."
   - "Moreover...", "Furthermore...", "In addition..."

   Write like a coworker leaving a quick note, not like a tutorial.

9. **Delete comments that add nothing.** If a comment restates the class or method name, delete it. `// Exception thrown when authentication fails` above `AuthenticationException` is pure noise.

10. **No `using Foo = Some.Fully.Qualified.Name;` aliases.** This is a project-wide rule, not just a comment one — fully-qualify inline or rename the conflicting type. See the project's memory for the reasoning.

11. **When unsure, ask.** If you can't decide whether a comment is needed or how to phrase it, ask the user before writing.

## When a comment is worth writing

Add a comment only when the reader would otherwise have to guess. Good reasons:

- **An invariant not visible in the signature.** "Only the latest report is kept; older rows are deleted on download."
- **A non-obvious constraint.** "Must stay in sync with `CliStrings.cs.resx` and `CliStrings.sk.resx`."
- **A workaround for subtle behavior.** "SQLite loses sub-millisecond precision on round-trip, so equality checks round first."
- **A security or ownership note.** "Role check still required here — the menu can hide the option but a direct send can bypass that."
- **An append-only enum warning.** "Never reorder or renumber. The integer values are stored in `audit_logs`."

Don't write a comment just because a field or method looks important. If the name carries the meaning, leave it alone.

## Examples

### Bad — XML markup and too long

```csharp
/// <summary>
/// Downloads the latest ARK report for an admin user and recalculates the diff
/// against the previously stored report.
///
/// This handler is invoked from <see cref="DownloadReportAction"/> in the CLI
/// and also from <see cref="AdminReportsController.Download"/> on the API
/// side. The result is wrapped in <see cref="Result{T}"/> on both paths.
/// </summary>
public sealed class DownloadLatestArkReportCommandHandler(...)
```

### Good — none

```csharp
public sealed class DownloadLatestArkReportCommandHandler(...)
```

The class and the command record name already say what it does. The XML doc adds nothing.

### Bad — restates the class name

```csharp
// Exception thrown when authentication fails.
public sealed class AuthenticationException : Exception
```

### Good — none

```csharp
public sealed class AuthenticationException : Exception
```

### Bad — em-dash and decorated references

```csharp
// Detail string — surfaces in [audit_logs.Detail]. Never include secrets
// like `appsettings.json` SMTP credentials here.
public string Detail => $"Recipients: {RecipientCount}";
```

### Good

```csharp
// Detail surfaces in audit_logs. Don't include secrets like SMTP credentials.
public string Detail => $"Recipients: {RecipientCount}";
```

### Bad — historical narrative

```csharp
// Previously we stored a full report history but we removed it in
// milestone-2 because of disk space. Now we keep only the latest.
public Task<Report?> GetLatestAsync();
```

### Good

```csharp
// Only the latest report is kept; older rows are deleted on each download.
public Task<Report?> GetLatestAsync();
```

## XML doc comments already in the codebase

Some older files still use `/// <summary>` style. When you touch one of those files for any reason, convert the comments in the same pass. The goal is for the codebase to converge on `//`-only comments without doing a one-shot rewrite that touches every file.

## Quick checklist before committing a comment

- [ ] Uses `//`, no XML.
- [ ] No em-dash or en-dash characters.
- [ ] No brackets or backticks around variable/type names.
- [ ] Three lines or fewer.
- [ ] Describes the current state, not history.
- [ ] Would a reader actually need this note, or does the code already say it?

## Related skills

- [[clean-architecture]] — naming things well reduces the need for comments
- [[localization]] — comments are not user-facing and don't get localized
