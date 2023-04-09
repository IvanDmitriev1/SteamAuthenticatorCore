using System.Windows.Data;
using System.Windows.Markup;

namespace SteamAuthenticatorCore.Desktop.Helpers;

[MarkupExtensionReturnType(typeof(object))]
public sealed class LocalizationMarkupExtension : MarkupExtension
{
    public string LocalizationString { get; set; } = string.Empty;
    public LocalizationMessages LocalizationMessages { get; set; }
    public BindingMode BindingMode { get; set; } = BindingMode.OneWay;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

        if (!string.IsNullOrWhiteSpace(LocalizationString))
            dictionaryKey = LocalizationString;

        if (LocalizationMessages != LocalizationMessages.None)
            dictionaryKey = LocalizationMessages.ToString();

        var binding = new Binding()
        {
            Source = AppSettings.Current.LocalizationProvider,
            Path = new PropertyPath($"[{dictionaryKey}]"),
            Mode = BindingMode,
        };

        return binding.ProvideValue(serviceProvider);
    }
}