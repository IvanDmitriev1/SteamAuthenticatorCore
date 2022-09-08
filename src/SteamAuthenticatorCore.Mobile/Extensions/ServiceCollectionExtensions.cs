using SteamAuthenticatorCore.Mobile.Abstractions;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Mobile.Services;
using SteamAuthenticatorCore.Mobile.ViewModels;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Mobile.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection services)
    {
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

        return services;
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
        services.AddSingleton<IEnvironment, PlatformEnvironment>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPlatformImplementations, PlatformImplementations>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IConfirmationViewModelFactory, ConfirmationViewModelFactory>();
        services.AddTransient<IPlatformTimer, PeriodicTimerService>();
        services.AddScoped<SecureStorageService>();

        services.AddHttpClient<IUpdateService, UpdateService>();
    }
}
