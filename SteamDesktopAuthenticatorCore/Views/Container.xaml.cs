using System.Windows;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using WPFUI.DIControls.Interfaces;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class Container : Window
{
    public Container(AppSettings appSettings, INavigation navigation, IDialog dialog, ISnackbar snackbar)
    {
        _appSettings = appSettings;
        InitializeComponent();

        RootDialog.Content = dialog;
        RootSnackbar.Content = snackbar;

        Breadcrumb.Navigation = navigation;
        RootNavigation.Content = navigation;
        RootTitleBar.Navigation = navigation;

        navigation.AddFrame(RootFrame);
        _navigation = navigation;
    }

    private readonly AppSettings _appSettings;
    private readonly INavigation _navigation;


    private void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void TokenPage_OnClick(object sender, RoutedEventArgs e)
    {
        ShowWindow();
        _navigation.NavigateTo($"{nameof(TokenPage)}");
    }

    private void ConfirmationsPage_OnClick(object sender, RoutedEventArgs e)
    {
        ShowWindow();
        _navigation.NavigateTo($"{nameof(ConfirmationsPage)}");
    }

    private void SettingsPage_OnClick(object sender, RoutedEventArgs e)
    {
        ShowWindow();
        _navigation.NavigateTo($"{nameof(SettingsPage)}");
    }

    private void ShowWindow()
    {
        Show();

        WindowState = WindowState.Normal;
        Topmost = true;
        Topmost = false;

        Focus();
    }
}