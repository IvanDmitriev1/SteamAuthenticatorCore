using System.Windows.Controls;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
