---
name: database
description: Documents the EF Core + SQLite layer of Popocatepetl - DbContext, entity configurations, repository pattern, migrations rules, seeders, BaseEntity, and DateTime conventions. Use when adding/changing entities, writing repositories, or touching schema.
argument-hint: "[entity name, migration question, repository question]"
---

# Database

Persistence is **EF Core on SQLite**. The database lives at the path bound from `appsettings.json` key `Database:Path` (relative paths are resolved against `AppContext.BaseDirectory` — see `InfrastructureServiceExtensions.cs`).

## Layout

| Folder | What |
| --- | --- |
| `Popocatepetl.Infrastructure/Data/PopocatepetlDbContext.cs` | The `DbContext`. Has `DbSet<T>` per entity. |
| `Popocatepetl.Infrastructure/Data/Configurations/` | One `IEntityTypeConfiguration<T>` per entity. Fluent API only. |
| `Popocatepetl.Infrastructure/Data/Migrations/` | EF migrations + snapshot. **Never edit existing migration files.** |
| `Popocatepetl.Infrastructure/Data/Seeders/` | Idempotent async seeders called by `UseSeeding` / `UseAsyncSeeding`. |
| `Popocatepetl.Infrastructure/Repositories/` | `I*Repository` implementations from `Popocatepetl.Domain/Interfaces/`. |
| `Popocatepetl.Domain/Entities/` | The entities themselves. Plain C# — no EF attributes. |
| `Popocatepetl.Domain/Interfaces/` | Repository **ports** (interfaces). Implementations live in Infrastructure. |

## Entities and BaseEntity

Every persistent entity inherits from `BaseEntity` (`Popocatepetl.Domain/Entities/BaseEntity.cs`), which contributes the common columns shared across the schema (created/updated timestamps, identity). Concrete entities — `Report`, `DiffResult`, `DiffData`, `AppUser`, `AuditLog`, `AppSetting`, `MailAttachment` — add their own fields and factory methods.

**Conventions:**

- Plain C# class. **No `[Required]`, `[MaxLength]`, `[ForeignKey]`, or other data annotations.** Configure those in the Fluent API configuration class instead.
- Use a private setter or an `init`-only setter for fields that should not change after construction.
- Provide a static factory method (e.g. `Report.Create(...)`, `AuditLog.Create(...)`) that establishes invariants. Avoid public parameterless constructors; let EF use a non-public one if necessary.
- Domain entities may not reference EF Core types. If you need a value object, put it in `Popocatepetl.Domain/ValueObjects/`.

## Entity configurations

For each entity, drop a configuration class next to the others:

`Popocatepetl.Infrastructure/Data/Configurations/XxxConfiguration.cs`:

```csharp
internal sealed class XxxConfiguration : IEntityTypeConfiguration<Xxx>
{
    public void Configure(EntityTypeBuilder<Xxx> builder)
    {
        builder.ToTable("xxx");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.SomeColumn);
        // relationships, conversions, owned types, etc.
    }
}
```

Configurations are picked up automatically by `modelBuilder.ApplyConfigurationsFromAssembly(...)` inside `PopocatepetlDbContext`. **Add a `DbSet<Xxx>` property in the context** for the entity to appear.

Inherit shared bits from `BaseEntityConfiguration` where helpful — see `AppUserConfiguration` for the established pattern.

## Repository pattern

For each entity that needs queries beyond raw CRUD:

1. **Interface in Domain.** `Popocatepetl.Domain/Interfaces/IXxxRepository.cs`. Extend `IRepository<T>` if you want the standard CRUD slate.
2. **Implementation in Infrastructure.** `Popocatepetl.Infrastructure/Repositories/XxxRepository.cs`. Constructor-injects `PopocatepetlDbContext`.
3. **Register.** Add `services.AddScoped<IXxxRepository, XxxRepository>();` to `InfrastructureServiceExtensions.cs` next to the existing registrations.

Rules:

- **Always async.** All public methods return `Task` / `Task<T>` and accept `CancellationToken`.
- **Read methods use `AsNoTracking()`** unless the caller will mutate and save.
- **Never expose `IQueryable<T>` upward.** Materialize inside the repository — return `IReadOnlyList<T>`, `T?`, etc. This keeps EF out of the Application layer.
- **No side effects in `Get*` methods.** Reads are reads.

## DateTime handling

- Always **UTC**, always `DateTime.UtcNow`. Never `DateTime.Now`.
- Properties live as plain `DateTime` (not `DateTimeOffset`).
- When formatting for the user, convert at the presentation edge — handlers and repositories deal in UTC only.

## Migrations

The single hardest rule:

> **Never edit an existing migration file.** Migrations are an append-only ledger of schema changes. Once a migration has shipped to anyone's database (yours included, in a development environment), modifying it produces silent schema drift that EF will not detect.

To make any schema change:

```bash
# 1. Change the entity + its Configuration class
# 2. Add a new migration:
dotnet ef migrations add <DescriptiveName> \
  --project Popocatepetl.Infrastructure \
  --startup-project Popocatepetl.CLI

# 3. Inspect the generated *.cs migration. EF guesses well but not perfectly.
# 4. Run it as part of normal app startup (Program.cs calls Database.MigrateAsync).
```

Migration files live in `Popocatepetl.Infrastructure/Data/Migrations/`. The accompanying `PopocatepetlDbContextModelSnapshot.cs` is regenerated by the tooling — let it be.

If a migration is wrong and hasn't shipped anywhere, the cleanest fix is `dotnet ef migrations remove` + re-add. Once it has shipped, write a **new** migration that corrects course.

## Seeders

Seeders live in `Popocatepetl.Infrastructure/Data/Seeders/`. They are async and **must be idempotent** — running them twice on the same database should leave it unchanged.

```csharp
public static async Task SeedAsync(PopocatepetlDbContext db)
{
    if (await db.Reports.AnyAsync()) return;
    db.Reports.AddRange(...);
    await db.SaveChangesAsync();
}
```

They are wired into the DbContext options in `InfrastructureServiceExtensions.cs` via `.UseSeeding(...)` (sync) and `.UseAsyncSeeding(...)` (async). New seeders must be called from both delegates.

`SeedIds.cs` holds the stable GUIDs the seeders use, so seeded rows don't get fresh IDs each run.

## Soft delete vs hard delete

- The Popocatepetl schema currently uses **hard deletes** for reports (only the latest is kept) and audit logs (retained as a real history table).
- If you add an entity where deletion needs to be reversible or tracked, add an `IsDeleted` flag + a `DeletedAt` column rather than removing rows. Update the corresponding repository to filter on it.

## Migration startup

`Popocatepetl.CLI/Program.cs` calls:

```csharp
await sp.GetRequiredService<PopocatepetlDbContext>().Database.MigrateAsync(cts.Token);
```

This applies any pending migrations on boot. Users do not run `dotnet ef` themselves — they just launch the .exe. Keep migrations idempotent and fast.

## SQLite gotchas

- **`DateTime` round-trips lose sub-millisecond precision.** If tests assert exact timestamp equality, round to milliseconds first.
- **`Guid` is stored as TEXT.** Joins and `WHERE` clauses still work, but raw SQL must quote and match the canonical hyphenated form.
- **No `RIGHT JOIN`.** Rewrite as `LEFT JOIN` from the other side.
- **`Migrate()` is destructive only if you write destructive migrations.** Adding a non-nullable column without a default to a non-empty table will fail; supply a default value in the migration or split the change into add-nullable + backfill + alter-not-null.

## Adding a new entity — checklist

- [ ] `Popocatepetl.Domain/Entities/Xxx.cs` (plain C#, factory method, no EF attributes)
- [ ] `Popocatepetl.Domain/Interfaces/IXxxRepository.cs` (if it needs custom queries)
- [ ] `Popocatepetl.Infrastructure/Data/Configurations/XxxConfiguration.cs`
- [ ] `DbSet<Xxx>` property in `PopocatepetlDbContext`
- [ ] `Popocatepetl.Infrastructure/Repositories/XxxRepository.cs`
- [ ] `services.AddScoped<IXxxRepository, XxxRepository>()` in `InfrastructureServiceExtensions`
- [ ] `dotnet ef migrations add Add<Xxx>Table`
- [ ] Optional seeder + wiring in `UseSeeding` / `UseAsyncSeeding`
- [ ] Tests in `Popocatepetl.Infrastructure.Tests/`

## Related skills

- [[clean-architecture]] — why the interface lives in Domain
- [[add-feature]] — when you need an entity for a new feature
- [[testing]] — repository tests with SQLite in-memory
