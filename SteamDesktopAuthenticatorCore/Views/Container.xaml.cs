using System.Windows;
using SteamDesktopAuthenticatorCore.Common;
using SteamDesktopAuthenticatorCore.ViewModels;
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

            WPFUI.Background.Manager.Apply(this);

            RootDialog.Content = dialog;
            RootSnackbar.Content = snackbar;


            Breadcrumb.Navigation = navigation;
            RootNavigation.Content = navigation;
            RootTitleBar.Navigation = navigation;

            navigation.AddFrame(RootFrame);

            navigation.Navigated += NavigationOnNavigated;
        }

        private readonly AppSettings _appSettings;

        private void NavigationOnNavigated(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
