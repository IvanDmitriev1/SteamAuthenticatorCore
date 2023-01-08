namespace SteamAuthenticatorCore.Mobile.Helpers;

public static class ColorsCollection
{
    private static readonly Dictionary<string, (Color, Color)> Dictionary = new();

    public static void Add(string key, string lightColorKey, string darkColorKey)
    {
        Application.Current!.Resources.TryGetValue(lightColorKey, out var lightColor);
        Application.Current!.Resources.TryGetValue(darkColorKey, out var darkColor);

        Dictionary.Add(key, ((Color, Color))(lightColor, darkColor));
    }

    public static (Color, Color) GetTuple(string key) => Dictionary[key];

    public static Color Get(string key)
    {
        var colorsTuple = Dictionary[key];
        return Application.Current!.RequestedTheme == AppTheme.Light ? colorsTuple.Item1 : colorsTuple.Item2;
    }
}
