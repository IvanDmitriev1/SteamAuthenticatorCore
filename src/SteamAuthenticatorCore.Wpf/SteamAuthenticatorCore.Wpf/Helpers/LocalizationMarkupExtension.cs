using System.Windows.Data;
using System.Windows.Markup;

namespace SteamAuthenticatorCore.Desktop.Helpers;

[MarkupExtensionReturnType(typeof(object))]
public sealed class LocalizationMarkupExtension : MarkupExtension
{
    public string LocalizationString { get; set; } = string.Empty;
    public LocalizationMessages LocalizationMessages { get; set; }

    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        string dictionaryKey = string.Empty;

        if (!string.IsNullOrWhiteSpace(LocalizationString))
            dictionaryKey = LocalizationString;

        if (LocalizationMessages != LocalizationMessages.None)
            dictionaryKey = LocalizationMessages.ToString();

        var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!;

        if (target.TargetObject is not DependencyObject obj || target.TargetProperty is not DependencyProperty prop)
        {
            return null;
        }

        var binding = new Binding()
        {
            Source = AppSettings.Current.LocalizationProvider,
            Path = new PropertyPath($"[{dictionaryKey}]"),
            Mode = BindingMode.OneWay,
        };

         BindingOperations.SetBinding(obj, prop, binding);

        return binding.ProvideValue(serviceProvider);
    }
}