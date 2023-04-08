namespace SteamAuthenticatorCore.Shared.Abstractions;

public abstract class AutoSettings : ObservableObject, ISettings
{
    protected AutoSettings()
    {
        var properties = GetType().GetProperties().Where(info => info.GetCustomAttribute<IgnoreSetting>() is null).ToArray();

        PropertyInfos = properties;
        PropertiesDictionary = properties.ToDictionary(static info => info.Name);
    }

    protected readonly PropertyInfo[] PropertyInfos;
    protected readonly IReadOnlyDictionary<string, PropertyInfo> PropertiesDictionary;

    public abstract void Load();
    public abstract void Save();
}