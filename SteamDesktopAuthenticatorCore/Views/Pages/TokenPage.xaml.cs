using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class TokenPage : Page
    {
        public TokenPage()
        {
            DataContext = App.ServiceProvider.GetRequiredService<TokenViewModel>();
            InitializeComponent();
        }
    }
}
