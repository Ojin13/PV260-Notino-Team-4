---
name: testing
description: Documents the test stack for Popocatepetl (xUnit + FluentAssertions + Moq), folder conventions, the HandlerTestBase / MockedIocBuilder for handlers, and the ScriptedSelectPrompt / FakeStringLocalizer / TestConsole stack for CLI flow tests. Use when writing or reviewing any test.
argument-hint: "[layer, handler, menu action, or test scenario]"
---

# Testing

## The stack

Three projects, three layers, one toolchain:

| Project | Tests |
| --- | --- |
| `Popocatepetl.Application.Tests` | MediatR handlers, pipeline behaviors |
| `Popocatepetl.Infrastructure.Tests` | Repositories, EF queries, export/email adapters |
| `Popocatepetl.CLI.Tests` | Menu navigation, prompts, auth gate, persistence helpers |

All three use:

- **xUnit 2.9.3** as the runner. Test methods are `[Fact]` or `[Theory]`.
- **FluentAssertions 7** for assertions. Always assert with `.Should().…` chains.
- **Moq 4** for test doubles. `new Mock<IFoo>()`, `mock.Setup(...).Returns(...)`, `mock.Verify(...)`.

**Do not add NSubstitute, Shouldly, AutoFixture, or Bogus.** Keeping a single mocking library across the codebase is non-negotiable. The README mentions NSubstitute — it is wrong; the actual `.csproj` files reference Moq.

`GlobalUsings.cs` in each test project already imports `FluentAssertions`, `Moq`, `Xunit`, plus `MediatR` in the Application tests. **Do not re-import these in individual test files.**

## Folder conventions

Tests mirror the structure of the project they test:

```
Popocatepetl.Application/
    Handlers/Reports/DownloadLatestArkReportCommandHandler.cs
Popocatepetl.Application.Tests/
    Handlers/Reports/DownloadLatestArkReportCommandHandlerTests.cs
```

Each test file holds tests for one production type. Shared fakes live in `TestUtilities/` per test project.

## Naming

`MethodName_StateUnderTest_ExpectedBehavior` — verbose but searchable. Examples from the codebase:

```
RunAsync_PicksExitImmediately_ReturnsCleanly
RunAsync_RoleGatedChild_HiddenWhenSessionRoleTooLow
RunAsync_EntryGuardReturnsFalse_DoesNotPushBranch
```

## Handler tests

### When to extend `HandlerTestBase`

`Popocatepetl.Application.Tests/TestUtilities/HandlerTestBase.cs` spins up a real MediatR pipeline (so the `AuditLoggingBehavior` and friends actually run) with the most common dependencies mocked: `IEmailService`, `IDiffResultRepository`, `IDiffExporter`. Extend it when those mocks cover what you need:

```csharp
public sealed class SendEmailCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_RecipientsAreEmpty_ReturnsFailure()
    {
        var command = new SendEmailCommand([], "subject", "body");

        var result = await Mediator.Send(command);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("recipient");
        EmailService.Verify(
            s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MailAttachment?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

### When to build your own service provider

If your handler needs a port `HandlerTestBase` does not mock (an `IArkReportClient`, say), use `MockedIocBuilder` directly:

```csharp
var arkClient = new Mock<IArkReportClient>();
var serviceProvider = new MockedIocBuilder()
    .AddMockedReportRepository(out var reportRepo)
    .AddMocked(arkClient)            // hypothetical extension
    .AddMediatR(typeof(DependencyInjection).Assembly)
    .Build();
```

Add a new `AddMocked*` extension to `MockedIocBuilder` whenever you find yourself wiring the same mock twice. Tests should not be wallpapered with raw DI plumbing.

### What to assert

For every handler test, assert:

1. **The `Result` shape.** `result.IsSuccess.Should().BeTrue()` + `result.Value.Should()....` for success; `BeFalse()` + error message for failure.
2. **Side effects via `Verify`.** Did the repository get called once with the expected entity? Did the email service get a single attachment with the right filename?
3. **Negative paths.** Unauthorized caller should produce a failure result and **no** repository writes — verify with `Times.Never`.

### What not to mock

- `Result<T>` — it's a value type with no dependencies. Construct real ones.
- Domain entities (`Report`, `DiffResult`, `AppUser`). Use real ones; they have factory methods like `Report.Create(...)`.
- `Mediator` itself, when the test target is a CLI action sending a command — use the real `IMediator` from a `MockedIocBuilder`-built provider so the pipeline runs.

## CLI flow tests

The Spectre prompts are abstracted behind three interfaces — `ITextPrompt`, `ISelectPrompt`, `IConfirmPrompt` — so tests don't drive a real terminal. Use the scripted/fake versions from `Popocatepetl.CLI.Tests/TestUtilities/`.

### `ScriptedSelectPrompt`

Plays back a queue of pre-recorded picks and records what was offered. Use it to drive `StackNavigator`:

```csharp
var prompt = new ScriptedSelectPrompt("menu.exit");        // pick exit immediately
var prompt = new ScriptedSelectPrompt(                     // navigate two levels deep
    "menu.admin.title", "menu.back", "menu.exit");

prompt.AskedTitleKeys.Should().Equal(...);
prompt.OfferedChoiceLabels.Should().ContainSingle()
    .Which.Should().NotContain("admin.leaf");
```

### `FakeStringLocalizer<T>`

Returns the key verbatim as the localized value. Use it everywhere you'd otherwise inject `IStringLocalizer<CliStrings>` — comparisons in tests then run against the keys directly.

### `Spectre.Console.Testing.TestConsole`

Captures Spectre output and feeds a fake input channel. Inject it as `IAnsiConsole`:

```csharp
var console = new TestConsole();
var theme = new ThemeApplier();
var session = new CurrentUserContext { Email = "test@x.com", Role = UserRole.None };
var loc = new FakeStringLocalizer<CliStrings>();
var locale = new LocaleState();
var chrome = new MenuChrome(console, session, loc, theme);
var nav = new StackNavigator(prompt, session, chrome, console, locale);
```

`console.Output` returns everything that was written so you can assert specific markup or text appeared.

### Test pattern for a menu action

A typical CLI flow test:

1. Build the menu tree by hand using `MenuNode.Branch` / `MenuNode.Leaf` (don't use `TopMenuFactory` unless you're testing it specifically).
2. Build the navigator via a small `Build(prompt)` helper at the top of the test class — see `StackNavigatorTests.Build` for the pattern.
3. Drive `nav.RunAsync(root, default)`.
4. Assert on `prompt.AskedTitleKeys`, `prompt.OfferedChoiceLabels`, and any side effects on the action (a counting fake action is a common pattern — see `CountingAction` in `StackNavigatorTests`).

## Repository / Infrastructure tests

Live in `Popocatepetl.Infrastructure.Tests`. Use real EF Core with SQLite either in-memory (`new SqliteConnection("Filename=:memory:")`) or in a temp file. The Migration + Seeder pipeline applies just like in production. Assert against the real `DbContext` to make sure your repository actually persists what it claims to.

## Don't test

- Auto-generated EF migrations.
- Static helpers with no logic (constants, single-line property getters).
- `IRequestHandler` interfaces of MediatR or other framework types.

## Common assertions

```csharp
result.IsSuccess.Should().BeTrue();
result.Value.Should().NotBeNull();
result.Value!.SomeField.Should().Be("expected");

result.ErrorMessage.Should().Contain("not allowed");

mock.Verify(x => x.SomeMethod(It.Is<Entity>(e => e.Id == id)), Times.Once);
mock.VerifyNoOtherCalls();

asyncAct.Should().NotThrowAsync();
asyncAct.Should().ThrowExactlyAsync<DomainException>();
```

## Running tests

```bash
dotnet test                                        # whole solution
dotnet test Popocatepetl.Application.Tests         # one project
dotnet test --filter FullyQualifiedName~Reports    # by name pattern
```

CI runs `dotnet test` against the whole solution on every push. No test is allowed to be skipped (`[Fact(Skip="...")]`) without a tracking issue.

## Related skills

- [[clean-architecture]] — what depends on what
- [[error-handling]] — the failure modes you should be testing
- [[audit-logging]] — `HandlerTestBase` keeps the audit pipeline live
