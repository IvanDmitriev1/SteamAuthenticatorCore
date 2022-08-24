using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services.AddSingleton<AppSettings>();
        services.AddScoped<IConfirmationService, ConfirmationService>();
        services.AddScoped<ILoginService, LoginService>();
    }
}
