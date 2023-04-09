namespace SteamAuthenticatorCore.Maui.Extensions;

[ContentProperty(nameof(LocalizationMessages))]
public sealed class LocalizationMarkupExtension : IMarkupExtension<BindingBase>
{
    public LocalizationMarkupExtension()
    {

    }

    public LocalizationMarkupExtension(LocalizationMessages localizationMessages)
    {
        LocalizationMessages = localizationMessages;
    }

    public LocalizationMessages LocalizationMessages { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

        if (LocalizationMessages != LocalizationMessages.None)
            dictionaryKey = LocalizationMessages.ToString();

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