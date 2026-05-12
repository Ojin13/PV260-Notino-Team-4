using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Domain.Enums;
using Popocatepetl.Infrastructure.Data;
using Popocatepetl.Infrastructure.Repositories;

namespace Popocatepetl.CLI.Tests.Persistence;

public class AppSettingStoresTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PopocatepetlDbContext _db;

    public AppSettingStoresTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PopocatepetlDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new PopocatepetlDbContext(options);
        _db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task ThemeStore_GetActive_NoStoredValue_ReturnsDefault()
    {
        var store = new ThemeStore(new AppSettingRepository(_db));

        var palette = await store.GetActiveAsync();

        palette.Id.Should().Be(BuiltInPalettes.Default.Id);
    }

    [Fact]
    public async Task ThemeStore_SetThenGet_RoundTripsThePersistedValue()
    {
        var store = new ThemeStore(new AppSettingRepository(_db));

        await store.SetActiveAsync("monokai");
        var palette = await store.GetActiveAsync();

        palette.Id.Should().Be("monokai");
    }

    [Fact]
    public async Task ThemeStore_UnknownPersistedValue_FallsBackToDefault()
    {
        var repo = new AppSettingRepository(_db);
        await repo.SetAsync(AppSettingType.ColorScheme, "no-such-palette");

        var palette = await new ThemeStore(repo).GetActiveAsync();

        palette.Id.Should().Be(BuiltInPalettes.Default.Id);
    }

    [Fact]
    public async Task LocaleStore_GetActive_NoStoredValue_ReturnsDefaultLocale()
    {
        var store = new LocaleStore(new AppSettingRepository(_db));

        var locale = await store.GetActiveAsync();

        locale.Should().Be(LocaleStore.DefaultLocale);
    }

    [Theory]
    [InlineData("cs")]
    [InlineData("sk")]
    [InlineData("en")]
    public async Task LocaleStore_SetThenGet_RoundTripsForSupportedLocales(string tag)
    {
        var store = new LocaleStore(new AppSettingRepository(_db));

        await store.SetActiveAsync(tag);
        var locale = await store.GetActiveAsync();

        locale.Should().Be(tag);
    }

    [Fact]
    public async Task LocaleStore_UnsupportedPersistedValue_FallsBackToDefault()
    {
        var repo = new AppSettingRepository(_db);
        await repo.SetAsync(AppSettingType.Language, "fr");

        var locale = await new LocaleStore(repo).GetActiveAsync();

        locale.Should().Be(LocaleStore.DefaultLocale);
    }

    [Fact]
    public async Task AppSettingRepository_SetTwice_UpdatesExistingRow()
    {
        var repo = new AppSettingRepository(_db);

        await repo.SetAsync(AppSettingType.ColorScheme, "first");
        await repo.SetAsync(AppSettingType.ColorScheme, "second");

        (await repo.GetAsync(AppSettingType.ColorScheme)).Should().Be("second");
    }
}
