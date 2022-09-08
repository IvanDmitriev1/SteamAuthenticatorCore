using CommunityToolkit.Maui;
using SteamAuthCore.Extensions;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Shared.Extensions;
using Syncfusion.Maui.Core.Hosting;

namespace SteamAuthenticatorCore.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MaterialIcons-Regular.ttf", "Material");
            });

        builder.Services.AddSteamAuthCoreServices();
        builder.Services.AddSharedServices();
        builder.Services.AddAllServices();

        return builder.Build();
    }
}