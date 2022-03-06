using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using SteamAuthenticatorCore.Shared;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services
{
    internal class MobileSettingsService : ISettingsService
    {
        private const string FileName = "settings.json";
        private static readonly string SettingsFile = Path.Combine(FileSystem.AppDataDirectory, FileName);

        public async void LoadSettings(ISettings settings)
        {
            if (!File.Exists(SettingsFile))
                SaveSettings(settings);

            var type = settings.GetType();
            var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null).ToArray();

            Dictionary<string, JsonValue> obj;

            using (var stream = File.OpenRead(SettingsFile))
            {
                try
                {
                    obj = (await JsonSerializer.DeserializeAsync<Dictionary<string, JsonValue>>(stream))!;
                }
                catch (Exception)
                {
                    obj = new Dictionary<string, JsonValue>();
                }   
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

        public async void SaveSettings(ISettings settings)
        {
            var type = settings.GetType();
            var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null);

            Dictionary<string, object> obj = properties.ToDictionary(property => property.Name, property => property.GetValue(settings) ?? property.PropertyType);

            using var stream = new FileStream(SettingsFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(JsonSerializer.Serialize(obj));
        }
    }
}
