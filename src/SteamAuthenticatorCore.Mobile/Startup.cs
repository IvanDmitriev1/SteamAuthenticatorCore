using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore;
using SteamAuthCore.Extensions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Mobile.Services;
using SteamAuthenticatorCore.Mobile.ViewModels;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Extensions;
using SteamAuthenticatorCore.Shared.Models;

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
        services.AddSingleton<IPlatformImplementations, MobileImplementations>();
        services.AddScoped<SecureStorageService>();
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.AddScoped<IConfirmationViewModelFactory, ConfirmationViewModelFactory>();

        services.AddSteamAuthCoreServices();
        services.AddSharedServices();

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
}