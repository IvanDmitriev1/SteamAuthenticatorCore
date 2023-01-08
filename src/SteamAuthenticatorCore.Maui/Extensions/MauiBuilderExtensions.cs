using SteamAuthCore.Extensions;
using SteamAuthenticatorCore.Mobile.Abstractions;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Mobile.Services;
using SteamAuthenticatorCore.Mobile.ViewModels;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Extensions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Mobile.Extensions;

internal static class MauiBuilderExtensions
{
    public static MauiAppBuilder AddAllServices(this MauiAppBuilder builder)
    {
        var services = builder.Services;

        services.AddSteamAuthCoreServices();
        services.AddSharedServices();

        services.AddViewModels();
        services.AddServices();
        services.AddPages();

        services.AddSingleton<AccountsFileServiceResolver>(provider => () =>
        {
            var appSettings = provider.GetRequiredService<AppSettings>();
            return appSettings.AccountsLocation switch
            {
                AccountsLocationModel.LocalDrive => provider
                    .GetRequiredService<SecureStorageService>(),
                AccountsLocationModel.GoogleDrive => 
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
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ConfirmationsOverviewViewModel>();
        services.AddTransient<ConfirmationViewModel>();
    }

    private static void AddPages(this IServiceCollection services)
    {
        services.AddTransient<TokenPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<ConfirmationsOverviewPage>();
        services.AddTransient<ConfirmationsPage>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IStatusBar, StatusBar>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPlatformImplementations, PlatformImplementations>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddScoped<SecureStorageService>();

        services.AddHttpClient<IUpdateService, UpdateService>();
    }
}
