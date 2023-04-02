using CommunityToolkit.Maui;
using MaterialColorUtilities.Maui;
using Microsoft.Extensions.Logging;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Mobile.Resources;
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
            .UseMaterialColors<CustomMaterialColorService>()
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
            }).ConfigureEssentials(essentialsBuilder =>
            {
                essentialsBuilder.UseVersionTracking();
            });

        builder.Services.AddLogging(loggingBuilder =>
        {
#if DEBUG
            loggingBuilder.AddDebug();
#else
            loggingBuilder.AddSentry();
#endif
        });

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHFqVkNrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRcQl5hTH5adkxiWHhecXA=;Mgo+DSMBPh8sVXJ1S0d+X1RPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSX1Rc0VqWXZfcXNQRGQ=;ORg4AjUWIQA/Gnt2VFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5QdEBjUH9XcXVdR2NY;MTQ2NjM4NEAzMjMxMmUzMTJlMzMzNWtRZkdqZFlUdUY0UFhjVGdKeHpha3EyUSszMlJxUFNBSklBU1R0eExOVEU9;MTQ2NjM4NUAzMjMxMmUzMTJlMzMzNU1GKzdRaE9IUGFSUmVTTXljN2R4eHA1MXFWWjJURmhCUk5oZG5wRTV4Tmc9;NRAiBiAaIQQuGjN/V0d+XU9Hc1RDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31TdUdmWXZeeXVUTmlZVQ==;MTQ2NjM4N0AzMjMxMmUzMTJlMzMzNVhzbmNaRldhTS9pOTFFWm15eENjeEZQb0dkLzVoa3pzZi9RQnUyUjluWlE9;MTQ2NjM4OEAzMjMxMmUzMTJlMzMzNWFza3Jzc2Y5QzVHUm4vNVgxYklqVzl5Ymc3L2Z5dHVkNDFhcU1ZQ0o1QVU9;Mgo+DSMBMAY9C3t2VFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5QdEBjUH9XcXVcTmJY;MTQ2NjM5MEAzMjMxMmUzMTJlMzMzNUI5aXA0MzR6d21sNGZPc3k3ZUFvd0V2eXhvZXFQQ2ZUelVWbmQ5NHcybzQ9;MTQ2NjM5MUAzMjMxMmUzMTJlMzMzNW1EVzhISklpQkJGUGtKM3duaVN2ZWhyMEg1MnVLSDlDbnl6NCtKZFExcXc9;MTQ2NjM5MkAzMjMxMmUzMTJlMzMzNVhzbmNaRldhTS9pOTFFWm15eENjeEZQb0dkLzVoa3pzZi9RQnUyUjluWlE9");

        var settings = MauiAppSettings.Current;

        return builder.Build();
    }
}