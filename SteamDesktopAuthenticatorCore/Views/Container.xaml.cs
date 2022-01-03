using System.Windows;
using SteamDesktopAuthenticatorCore.Common;
using WPFUI.Controls;
using WPFUI.Controls.Navigation;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class Container : Window
    {
        public Container(AppSettings appSettings, DefaultNavigation navigation, Dialog dialog, Snackbar snackbar)
        {
            InitializeComponent();

            _appSettings = appSettings;
            WPFUI.Background.Manager.Apply(this);

            RootDialog.Content = dialog;
            RootSnackbar.Content = snackbar;


            Breadcrumb.Navigation = navigation;
            RootNavigation.Content = navigation;
            RootTitleBar.Navigation = navigation;

            navigation.AddFrame(RootFrame);
        }

        private readonly AppSettings _appSettings;
    }
}
