using System.Xml;

namespace SteamAuthenticatorCore.Shared.Services;

public sealed class XmlLocalizationProvider : ILocalizationProvider
{
    public XmlLocalizationProvider(AvailableLanguages language)
    {
        _currentLanguage = language;
        CurrentLanguageDictionary = LoadLanguageFromAssembly(language);
    }

    public IReadOnlyDictionary<string, string> CurrentLanguageDictionary { get; private set; }
    private AvailableLanguages _currentLanguage;

    private static string GetAssemblyName { get; } = Assembly.GetAssembly(typeof(XmlLocalizationProvider))!.GetName().Name!;

    public void ChangeLanguage(AvailableLanguages language)
    {
        if (_currentLanguage == language)
            return;

        _currentLanguage = language;
        CurrentLanguageDictionary = LoadLanguageFromAssembly(language);
    }

    private static IReadOnlyDictionary<string, string> LoadLanguageFromAssembly(AvailableLanguages language)
    {
        var stream = Assembly.GetAssembly(typeof(XmlLocalizationProvider))!.GetManifestResourceStream($"{GetAssemblyName}.Localization.{language}.xml")!;

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