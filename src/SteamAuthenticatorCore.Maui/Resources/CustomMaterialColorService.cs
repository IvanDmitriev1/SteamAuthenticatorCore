using MaterialColorUtilities.Maui;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using Microsoft.Extensions.Options;

namespace SteamAuthenticatorCore.Maui.Resources;

public class CustomMaterialColorService : MaterialColorService<CorePalette, Scheme<uint>, Scheme<Color>, LightSchemeMapper, DarkSchemeMapper>
{
    private readonly WeakEventManager _weakEventManager = new();
    
    public CustomMaterialColorService(IOptions<MaterialColorOptions> options, IDynamicColorService dynamicColorService, IPreferences preferences) : base(options, dynamicColorService, preferences)
    {
    }
    
    public event EventHandler SeedChanged
    {
        add => _weakEventManager.AddEventHandler(value);
        remove => _weakEventManager.RemoveEventHandler(value);
    }

    protected override async void Apply()
    {
        base.Apply();
        _weakEventManager.HandleEvent(null!, null!, nameof(SeedChanged));

#if ANDROID
        await Platform.WaitForActivityAsync().ConfigureAwait(false);
#endif

        StatusBar.SetColor(SchemeMaui.Surface2);
        StatusBar.SetStyle(IsDark ? StatusBarStyle.LightContent : StatusBarStyle.DarkContent);
    }
}