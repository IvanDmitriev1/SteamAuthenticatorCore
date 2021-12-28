using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<SettingsViewModel>();
        }
    }
}
