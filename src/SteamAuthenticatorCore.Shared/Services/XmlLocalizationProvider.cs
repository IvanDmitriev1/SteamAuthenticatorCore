using System.Xml;

namespace SteamAuthenticatorCore.Shared.Services;

public sealed class XmlLocalizationProvider : ILocalizationProvider
{
    public XmlLocalizationProvider(AvailableLanguage language)
    {
        _currentLanguage = language;

        _englishLanguageDictionary = LoadLanguageFromAssembly(AvailableLanguage.English);
        _currentLanguageDictionary = new Dictionary<string, string>(0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly IReadOnlyDictionary<string, string> _englishLanguageDictionary;
    private IReadOnlyDictionary<string, string> _currentLanguageDictionary;
    private AvailableLanguage _currentLanguage;

    private static string GetAssemblyName { get; } = Assembly.GetAssembly(typeof(XmlLocalizationProvider))!.GetName().Name!;

    public void ChangeLanguage(AvailableLanguage language)
    {
        if (_currentLanguage == language)
            return;

        if (language == AvailableLanguage.English)
            _currentLanguageDictionary = _englishLanguageDictionary;
        else
            _currentLanguageDictionary = LoadLanguageFromAssembly(language);

        _currentLanguage = language;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public string this[string key] => GetValue(key);

    public string this[LocalizationMessage message] => this[message.ToString()];

    private string GetValue(string key)
    {
        if (_currentLanguageDictionary.TryGetValue(key, out var result))
            return result;

        if (_englishLanguageDictionary.TryGetValue(key, out result))
            return result;

        return "NOT_FOUND";
    }

    private static IReadOnlyDictionary<string, string> LoadLanguageFromAssembly(AvailableLanguage language)
    {
        using var stream =
            Assembly.GetAssembly(typeof(XmlLocalizationProvider))!.GetManifestResourceStream(
                $"{GetAssemblyName}.Localization.{language}.xml")!;

        var dictionary = new Dictionary<string, string>();

        var document = new XmlDocument();
        document.Load(stream);

        foreach (XmlNode node in document.DocumentElement!.ChildNodes)
        {
            if (node.NodeType != XmlNodeType.Element)
                continue;

            var attribute = node.Attributes!["key"]!;
            dictionary.Add(attribute.Value, node.InnerText);
        }

        return dictionary;
    }
}