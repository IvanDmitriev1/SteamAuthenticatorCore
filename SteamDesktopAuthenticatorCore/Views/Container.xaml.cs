using System.Windows;
using WPFUI.Controls;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class Container : Window
    {
        public Container()
        {
            InitializeComponent();
        }

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
