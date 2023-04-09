using System.Windows.Data;
using System.Windows.Markup;

namespace SteamAuthenticatorCore.Desktop.Helpers;

[ContentProperty(nameof(LocalizationMessages))]
[MarkupExtensionReturnType(typeof(object))]
public sealed class LocalizationMarkupExtension : MarkupExtension
{
    public LocalizationMarkupExtension()
    {
    }

    public LocalizationMarkupExtension(LocalizationMessages localizationMessages)
    {
        LocalizationMessages = localizationMessages;
    }

    [ConstructorArgument("localizationMessages")]
    public LocalizationMessages LocalizationMessages { get; set; }

    public BindingMode BindingMode { get; set; } = BindingMode.OneTime;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

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