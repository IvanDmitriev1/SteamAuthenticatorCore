using SteamAuthenticatorCore.MobileMaui.Abstractions;
using SteamAuthenticatorCore.MobileMaui.Pages;
using SteamAuthenticatorCore.MobileMaui.Services;
using SteamAuthenticatorCore.MobileMaui.ViewModels;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.MobileMaui.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAllServices(this IServiceCollection services)
    {
        services.AddViewModels();
        services.AddServices();
        services.AddPages();

        return services;
    }

    private static void AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<SettingsViewModel>();
    }

    private static void AddPages(this IServiceCollection services)
    {
        services.AddTransient<SettingsPage>();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IEnvironment, PlatformEnvironment>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPlatformImplementations, PlatformImplementations>();
        services.AddSingleton<IUpdateService, UpdateService>();
    }
}
