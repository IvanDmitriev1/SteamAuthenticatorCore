using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileSettingsService : ISettingsService
{
    private const string FileName = "settings.json";
    private static readonly string SettingsFile = Path.Combine(FileSystem.AppDataDirectory, FileName);

    public void LoadSettings(ISettings settings)
    {
        throw new NotImplementedException();
    }

    public void SaveSettings(ISettings settings)
    {
        throw new NotImplementedException();
    }

    public async Task LoadSettingsAsync(ISettings settings)
    {
        if (!File.Exists(SettingsFile))
            await SaveSettingsAsync(settings);

        var type = settings.GetType();
        var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null).ToArray();

        Dictionary<string, JsonValue> obj;

        try
        {
            await using var stream = File.OpenRead(SettingsFile);
            obj = (await JsonSerializer.DeserializeAsync<Dictionary<string, JsonValue>>(stream))!;
        }
        catch (Exception)
        {
            obj = new Dictionary<string, JsonValue>();
            await SaveSettingsAsync(settings);
        }

        foreach (var property in properties)
        {
            if (!obj.ContainsKey(property.Name)) continue;

            object typeValue = obj[property.Name].ToJsonString();

            try
            {
                typeValue = property.PropertyType.IsEnum ? Enum.Parse(property.PropertyType, (string) typeValue) : Convert.ChangeType(typeValue, property.PropertyType);
                property.SetValue(settings, typeValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw new Exception("Failed to load setting");
            }
        }
    }

    public async Task SaveSettingsAsync(ISettings settings)
    {
        var type = settings.GetType();
        var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null);

        Dictionary<string, object> obj = properties.ToDictionary(property => property.Name, property => property.GetValue(settings) ?? property.PropertyType);

        await using var stream = File.OpenWrite(SettingsFile);
        await using var streamWriter = new StreamWriter(stream);
        await streamWriter.WriteAsync(JsonSerializer.Serialize(obj));
    }

}