using System.Windows.Input;

namespace SteamAuthenticatorCore.Mobile.Controls;

public class MyTitleView : ContentView
{
    public static readonly BindableProperty TitleNameProperty =
        BindableProperty.Create(nameof(TitleName), typeof(string), typeof(MyTitleView), string.Empty, BindingMode.OneTime);

    public static readonly BindableProperty IsContentVisibleProperty =
        BindableProperty.Create(nameof(IsContentVisible), typeof(bool), typeof(MyTitleView), false);

    public static readonly BindableProperty CloseCommandProperty = 
        BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(MyTitleView), null, BindingMode.OneTime);

    public static readonly BindableProperty IsCloseButtonVisibleProperty =
        BindableProperty.Create(nameof(IsCloseButtonVisible), typeof(bool), typeof(MyTitleView), true);

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

    public bool IsCloseButtonVisible
    {
        get => (bool)GetValue (IsCloseButtonVisibleProperty);
        set => SetValue (IsCloseButtonVisibleProperty, value);
    }
}
