using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Services;
using SteamAuthenticatorCore.Mobile.ViewModels;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Mobile;

public static class Startup
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static IServiceProvider Init(Action<IServiceCollection> nativeConfiguration)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.ConfigureServices().ConfigureViewModels();
        nativeConfiguration.Invoke(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();
        return ServiceProvider;
    }

    private static IServiceCollection ConfigureViewModels(this IServiceCollection services)
    {
        services.AddTransient<TokenPageViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<LoginViewModel>();

        return services;
    }

    private static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddTransient<IPlatformTimer, MobileTimer>();
        services.AddTransient<ISettingsService, MobileSettingsService>();
        services.AddTransient<IManifestDirectoryService, MobileDirectoryService>();
        services.AddSingleton<IPlatformImplementations, MobileImplementations>();
        services.AddScoped<IManifestModelService, SecureStorageService>();
        services.AddSingleton<ObservableCollection<SteamGuardAccount>>();

        services.AddTransient<LoginService>();
        services.AddSingleton<AppSettings>();
        services.AddSingleton<TokenService>();
        services.AddSingleton<BaseConfirmationService, MobileConfirmationService>();

        return services;
    }
}