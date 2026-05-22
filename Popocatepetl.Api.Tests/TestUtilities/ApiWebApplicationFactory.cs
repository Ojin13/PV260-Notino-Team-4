using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;
using Popocatepetl.Infrastructure.Data;

namespace Popocatepetl.Api.Tests.TestUtilities;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly string _databaseDirectory = Path.Combine(Path.GetTempPath(), $"popocatepetl-api-tests-{Guid.NewGuid():N}");
    private readonly string _databasePath;

    public ApiWebApplicationFactory()
    {
        Directory.CreateDirectory(_databaseDirectory);
        _databasePath = Path.Combine(_databaseDirectory, "popocatepetl.db");
    }

    public string LatestArkCsv { get; set; } = """
        company,ticker,shares,weight (%)
        TSLA,Tesla,10,1.5%
        """;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Path"] = _databasePath,
                ["Resend:ApiKey"] = "test-key",
                ["Resend:FromAddress"] = "test@example.com",
                ["ArkReports:LatestHoldingsUrl"] = "https://example.test/ark.csv"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<PopocatepetlDbContext>>();
            services.RemoveAll<PopocatepetlDbContext>();
            services.AddDbContext<PopocatepetlDbContext>(options =>
                options.UseSqlite($"Data Source={_databasePath}"));

            services.RemoveAll<IArkReportClient>();
            services.AddSingleton<IArkReportClient>(new StubArkReportClient(() => LatestArkCsv));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PopocatepetlDbContext>();

        await db.DiffData.ExecuteDeleteAsync();
        await db.DiffResults.ExecuteDeleteAsync();
        await db.Reports.ExecuteDeleteAsync();
        await db.AuditLogs.ExecuteDeleteAsync();
    }

    public async Task SeedReportAsync(Report report)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PopocatepetlDbContext>();
        db.Reports.Add(report);
        await db.SaveChangesAsync();
    }

    public async Task<T> WithDbContextAsync<T>(Func<PopocatepetlDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PopocatepetlDbContext>();
        return await action(db);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (Directory.Exists(_databaseDirectory))
        {
            try
            {
                Directory.Delete(_databaseDirectory, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }

    private sealed class StubArkReportClient(Func<string> csvFactory) : IArkReportClient
    {
        public Task<string> DownloadLatestAsync(CancellationToken cancellationToken)
            => Task.FromResult(csvFactory());
    }
}
