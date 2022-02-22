using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Mobile.Services
{
    internal class MobileSettingsService : ISettingsService
    {
        private const string FileName = "settings.json";
        private static readonly string FileLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public async void LoadSettings(ISettings settings)
        {
            string path = Path.Combine(FileLocation, FileName);

            if (!File.Exists(path))
                SaveSettings(settings);

            var type = settings.GetType();
            var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null).ToArray();

            using var stream = new FileStream(path, FileMode.Open);
            var obj = (await JsonSerializer.DeserializeAsync<Dictionary<string, JsonValue>>(stream))!;

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
            string path = Path.Combine(FileLocation, FileName);

            var type = settings.GetType();
            var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null);

            Dictionary<string, object> obj = properties.ToDictionary(property => property.Name, property => property.GetValue(settings) ?? property.PropertyType);

            using var stream = new FileStream(path, FileMode.OpenOrCreate);
            using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(JsonSerializer.Serialize(obj));
        }
    }
}
