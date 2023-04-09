using System.Windows.Data;
using System.Windows.Markup;

namespace SteamAuthenticatorCore.Desktop.Helpers;

[ContentProperty(nameof(LocalizationMessage))]
[MarkupExtensionReturnType(typeof(object))]
public sealed class LocalizationMarkupExtension : MarkupExtension
{
    public LocalizationMarkupExtension()
    {
    }

    public LocalizationMarkupExtension(LocalizationMessage localizationMessage)
    {
        LocalizationMessage = localizationMessage;
    }

    [ConstructorArgument("localizationMessage")]
    public LocalizationMessage LocalizationMessage { get; set; }

    public BindingMode BindingMode { get; set; } = BindingMode.OneTime;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

        if (LocalizationMessage != LocalizationMessage.None)
            dictionaryKey = LocalizationMessage.ToString();

        var binding = new Binding()
        {
            Source = AppSettings.Current.LocalizationProvider,
            Path = new PropertyPath($"[{dictionaryKey}]"),
            Mode = BindingMode,
        };

        return binding.ProvideValue(serviceProvider);
    }
}