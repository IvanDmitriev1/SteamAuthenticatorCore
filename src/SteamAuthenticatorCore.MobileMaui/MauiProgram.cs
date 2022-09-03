using SteamAuthenticatorCore.MobileMaui.Extensions;
using Syncfusion.Maui.Core.Hosting;

namespace SteamAuthenticatorCore.MobileMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MaterialIcons-Regular.ttf", "Material");
            });

        builder.Services.AddAllServices();

        return builder.Build();
    }
}