using System.Windows;
using SteamDesktopAuthenticatorCore.Common;
using SteamDesktopAuthenticatorCore.Views.Pages;
using WPFUI.Controls;
using WPFUI.Controls.Navigation;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class Container : Window
    {
        public Container(AppSettings appSettings, DefaultNavigation navigation, Dialog dialog, Snackbar snackbar)
        {
            _appSettings = appSettings;
            InitializeComponent();

            RootDialog.Content = dialog;
            RootSnackbar.Content = snackbar;

            Breadcrumb.Navigation = navigation;
            RootNavigation.Content = navigation;
            RootTitleBar.Navigation = navigation;

            navigation.AddFrame(RootFrame);
            navigation.Navigated += NavigationOnNavigated;
            _navigation = navigation;
        }

        private readonly AppSettings _appSettings;
        private readonly INavigation _navigation;

        private void NavigationOnNavigated(object sender, RoutedEventArgs e)
        {
            
        }

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
}
