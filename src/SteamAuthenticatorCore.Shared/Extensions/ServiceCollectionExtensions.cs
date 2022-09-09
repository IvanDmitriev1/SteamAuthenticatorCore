using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<AppSettings>();
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.AddSingleton<ObservableCollection<SteamGuardAccount>>();

        services.AddScoped<IConfirmationService, ConfirmationService>();
        services.AddScoped<ILoginService, LoginService>();

        services.AddTransient<ITimer, BackgroundTaskService>();
    }
}
