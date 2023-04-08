namespace SteamAuthenticatorCore.Maui.Extensions;

public sealed class LocalizationMarkupExtension : IMarkupExtension<BindingBase>
{
    public string LocalizationString { get; set; } = string.Empty;
    public LocalizationMessages LocalizationMessages { get; set; }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

        if (!string.IsNullOrWhiteSpace(LocalizationString))
            dictionaryKey = LocalizationString;

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