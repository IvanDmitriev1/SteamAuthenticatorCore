﻿using CommunityToolkit.Maui;
using SteamAuthenticatorCore.Mobile.Extensions;
using Syncfusion.Maui.Core.Hosting;

namespace SteamAuthenticatorCore.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddAllServices()
            .ConfigureSyncfusionCore()
            .UseMauiCommunityToolkit()
            .UseSentry(options =>
            {
                options.Dsn = "https://4a0459ed781e49c28e6c8e85da244344@o1354225.ingest.sentry.io/6658167";
                options.IncludeTitleInBreadcrumbs = true;
                options.IncludeTextInBreadcrumbs = true;
                options.IncludeBackgroundingStateInBreadcrumbs = true;
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MaterialIcons-Regular.ttf", "Material");
            });

        return builder.Build();
    }
}