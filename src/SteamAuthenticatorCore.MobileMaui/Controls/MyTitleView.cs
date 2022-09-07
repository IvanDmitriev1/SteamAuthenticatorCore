using System.Windows.Input;

namespace SteamAuthenticatorCore.MobileMaui.Controls;

public class MyTitleView : ContentView
{
    public static readonly BindableProperty TitleNameProperty =
        BindableProperty.Create(nameof(TitleName), typeof(string), typeof(MyTitleView), string.Empty, BindingMode.OneTime);

    public static readonly BindableProperty IsContentVisibleProperty =
        BindableProperty.Create(nameof(IsContentVisible), typeof(bool), typeof(MyTitleView), false);

    public static readonly BindableProperty CloseCommandProperty = 
        BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(MyTitleView), null, BindingMode.OneTime);

    public string TitleName
    {
        get => (string)GetValue (TitleNameProperty);
        set => SetValue (TitleNameProperty, value);
    }

    public bool IsContentVisible
    {
        get => (bool)GetValue (IsContentVisibleProperty);
        set => SetValue (IsContentVisibleProperty, value);
    }

    public ICommand? CloseCommand
    {
        get => (ICommand)GetValue (CloseCommandProperty);
        set => SetValue (CloseCommandProperty, value);
    }
}
