using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class TokenPage : Page
    {
        public TokenPage(TokenViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
