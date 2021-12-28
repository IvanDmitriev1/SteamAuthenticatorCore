using System.Windows;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class InitializingWindow : Window
    {
        public InitializingWindow()
        {
            InitializeComponent();
            DataContext = App.ViewModels[typeof(InitializingWindow)];
        }
    }
}
