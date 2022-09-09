using CommunityToolkit.Maui.Extensions;
using SteamAuthenticatorCore.Mobile.Helpers;

namespace SteamAuthenticatorCore.Mobile.Extensions;

internal static class VisualElementExtensions
{
    public static async ValueTask ChangeBackgroundColorToWithColorsCollection(this VisualElement visualElement, string key)
    {
        var colorTuple = ColorsCollection.GetTuple(key);
        await visualElement.BackgroundColorTo(Application.Current!.RequestedTheme == AppTheme.Light ? colorTuple.Item1 : colorTuple.Item2, 16, 150);

        visualElement.SetAppThemeColor(VisualElement.BackgroundColorProperty, colorTuple.Item1, colorTuple.Item2);
    }
}
