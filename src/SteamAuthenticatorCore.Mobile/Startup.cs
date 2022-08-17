using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Services;
using SteamAuthenticatorCore.Mobile.ViewModels;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Services;

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
        services.AddTransient<ConfirmationsOverviewViewModel>();
        services.AddTransient<ConfirmationViewModel>();

        return services;
    }

    private static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<AppSettings>();
        services.AddSingleton<ObservableCollection<SteamGuardAccount>>();
        services.AddTransient<IPlatformTimer, MobileTimer>();
        services.AddTransient<ISettingsService, MobileSettingsService>();
        services.AddTransient<IManifestDirectoryService, MobileDirectoryService>();
        services.AddSingleton<IPlatformImplementations, MobileImplementations>();
        services.AddScoped<IManifestModelService, SecureStorageService>();
        services.AddScoped<ConfirmationServiceBase, MobileConfirmationService>();
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.AddScoped<LoginService>();
        services.AddSingleton<ManifestAccountsWatcherService>();

        services.AddSingleton<ManifestServiceResolver>(provider => () =>
        {
            var appSettings = provider.GetRequiredService<AppSettings>();
            return appSettings.ManifestLocation switch
            {
                AppSettings.ManifestLocationModel.LocalDrive => provider
                    .GetRequiredService<IManifestModelService>(),
                AppSettings.ManifestLocationModel.GoogleDrive => 
                    throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException()
            };
        });

        return services;
    }
}