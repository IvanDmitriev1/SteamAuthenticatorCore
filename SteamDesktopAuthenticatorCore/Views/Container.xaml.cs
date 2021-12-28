using System.Windows;
using SteamDesktopAuthenticatorCore.Common;
using WpfHelper.Services;
using WPFUI.Controls;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class Container : Window
    {
        public Container(SettingService settingService)
        {
            _appSettings = settingService.Get<AppSettings>();
            WPFUI.Background.Manager.Apply(this);

            InitializeComponent();
        }

        private readonly AppSettings _appSettings;

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            var navigation = (NavigationFluent)(sender);
            navigation.Navigate("Token");
        }

        private void RootNavigation_OnNavigated(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
