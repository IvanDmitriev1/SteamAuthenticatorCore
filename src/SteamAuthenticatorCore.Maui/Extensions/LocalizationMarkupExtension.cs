namespace SteamAuthenticatorCore.Maui.Extensions;

[ContentProperty(nameof(LocalizationMessage))]
public sealed class LocalizationMarkupExtension : IMarkupExtension<BindingBase>
{
    public LocalizationMarkupExtension()
    {

    }

    public LocalizationMarkupExtension(LocalizationMessage localizationMessage)
    {
        LocalizationMessage = localizationMessage;
    }

    public LocalizationMessage LocalizationMessage { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

        if (LocalizationMessage != LocalizationMessage.None)
            dictionaryKey = LocalizationMessage.ToString();

        return new Binding()
        {
            Source = AppSettings.Current.LocalizationProvider,
            Path = $"[{dictionaryKey}]",
            Mode = BindingMode.OneWay
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}