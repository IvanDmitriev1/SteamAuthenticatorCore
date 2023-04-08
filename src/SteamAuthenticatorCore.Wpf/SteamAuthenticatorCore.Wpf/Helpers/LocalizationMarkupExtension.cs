using System.Windows.Markup;

namespace SteamAuthenticatorCore.Desktop.Helpers;

[MarkupExtensionReturnType(typeof(string))]
public sealed class LocalizationMarkupExtension : MarkupExtension
{
    public string LocalizationString { get; set; } = string.Empty;
    public LocalizationMessages LocalizationMessages { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryString = string.Empty;

        if (!string.IsNullOrWhiteSpace(LocalizationString))
            dictionaryString = LocalizationString;

        if (LocalizationMessages != LocalizationMessages.None)
            dictionaryString = LocalizationMessages.ToString();

        if (!AppSettings.Current.LocalizationProvider.CurrentLanguageDictionary.TryGetValue(dictionaryString, out var result))
            return string.Empty;

        return result;
    }
}