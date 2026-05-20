using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popocatepetl.Application.Common;
using Popocatepetl.CLI.Auth;
using Popocatepetl.CLI.Localization;
using Popocatepetl.CLI.Menus;
using Popocatepetl.CLI.Menus.Admin;
using Popocatepetl.CLI.Menus.PowerUser;
using Popocatepetl.CLI.Menus.Settings;
using Popocatepetl.CLI.Menus.User;
using Popocatepetl.CLI.Navigation;
using Popocatepetl.CLI.Prompts;
using Popocatepetl.CLI.Prompts.Native;
using Popocatepetl.CLI.Prompts.Spectre;
using Popocatepetl.CLI.Prompts.Stub;
using Popocatepetl.CLI.Services;
using Popocatepetl.CLI.Theming;
using Spectre.Console;

namespace Popocatepetl.CLI;

public static class DependencyInjection
{
    public static IServiceCollection AddCli(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AdminOptions>()
            .Bind(configuration.GetSection(AdminOptions.SectionName));

        services.AddLocalization();

        services.AddSingleton<CurrentUserContext>();
        services.AddSingleton<ICurrentUserContext>(sp => sp.GetRequiredService<CurrentUserContext>());

        services.AddSingleton<IAnsiConsole>(_ => AnsiConsole.Console);
        services.AddSingleton<IFileSaveDialog, NativeFileSaveDialog>();

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

        services.AddSingleton<StackNavigator>();
        services.AddSingleton<MenuChrome>();
        services.AddSingleton<AdminPasswordGate>();

        services.AddSingleton<ThemeApplier>();
        services.AddScoped<ThemeStore>();
        services.AddSingleton<LocaleState>();
        services.AddScoped<LocaleStore>();

        services.AddSingleton<EmailLoginPrompt>();
        services.AddScoped<ViewLatestDiffAction>();
        services.AddScoped<ChangeThemeAction>();
        services.AddScoped<SendReportAction>();
        services.AddScoped<DownloadReportAction>();
        services.AddScoped<ExportDiffAction>();
        services.AddScoped<ViewAuditLogsAction>();
        services.AddScoped<LanguageMenuAction>();
        services.AddScoped<TopMenuFactory>();

        return services;
    }
}
