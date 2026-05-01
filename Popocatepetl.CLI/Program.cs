using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Popocatepetl.Application;
using Popocatepetl.CLI;
using Popocatepetl.CLI.Menus;
using System.Globalization;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Theming;
using Popocatepetl.Infrastructure;
using Popocatepetl.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddCli();

using var host = builder.Build();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

await using var scope = host.Services.CreateAsyncScope();
var sp = scope.ServiceProvider;

await sp.GetRequiredService<PopocatepetlDbContext>().Database.MigrateAsync(cts.Token);

var palette = await sp.GetRequiredService<ThemeStore>().GetActiveAsync(cts.Token);
sp.GetRequiredService<ThemeApplier>().Apply(palette);

var locale = await sp.GetRequiredService<LocaleStore>().GetActiveAsync(cts.Token);
sp.GetRequiredService<LocaleState>().Set(CultureInfo.GetCultureInfo(locale));

await sp.GetRequiredService<EmailLoginPrompt>().RunAsync(cts.Token);

var root = sp.GetRequiredService<TopMenuFactory>().Build();
await sp.GetRequiredService<StackNavigator>().RunAsync(root, cts.Token);
