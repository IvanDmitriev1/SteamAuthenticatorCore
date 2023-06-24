namespace SteamAuthenticatorCore.Maui.Extensions;

internal static class MauiBuilderExtensions
{
    public static MauiAppBuilder AddAllServices(this MauiAppBuilder builder)
    {
        var services = builder.Services;

        services.AddSteamAuthCoreServices(false);
        services.AddSharedServices();

        services.AddViewModels();
        services.AddServices();
        services.AddPages();

        services.AddSingleton<AccountsServiceResolver>(static provider => () =>
        {
            var appSettings = AppSettings.Current;

            return appSettings.AccountsLocation switch
            {
                AccountsLocation.LocalDrive => provider
                    .GetRequiredService<SqLiteLocalStorageService>(),
                AccountsLocation.GoogleDrive => 
                    throw new NotImplementedException(),

                _ => throw new ArgumentOutOfRangeException()
            };
        });

        return builder;
    }

    private static void AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<TokenViewModel>();
        services.AddTransient<ConfirmationsOverviewViewModel>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<AccountConfirmationsViewModel>();
        services.AddTransient<AvailableLanguagesPageViewModel>();
    }

    private static void AddPages(this IServiceCollection services)
    {
        services.AddTransient<TokenPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<ConfirmationsOverviewPage>();
        services.AddTransient<AccountConfirmationsPage>();
        services.AddTransient<AvailableLanguagesPage>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformImplementations, PlatformImplementations>();
        services.AddSingleton<IUpdateService, UpdateService>(_ =>
            new UpdateService(Version.Parse(VersionTracking.CurrentVersion)));

        services.AddSingleton<SqLiteLocalStorageService>();
    }
}
