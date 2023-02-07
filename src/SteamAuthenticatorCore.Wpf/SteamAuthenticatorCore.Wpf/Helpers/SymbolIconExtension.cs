using System;
using System.Windows.Markup;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace SteamAuthenticatorCore.Desktop.Helpers;

internal class SymbolIconExtension : MarkupExtension
{
    public SymbolRegular Symbol { get; set; }
    public bool Filled { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        new SymbolIcon()
        {
            Symbol = Symbol,
            Filled = Filled
        };
}