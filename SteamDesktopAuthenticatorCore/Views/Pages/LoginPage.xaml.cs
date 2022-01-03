using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
