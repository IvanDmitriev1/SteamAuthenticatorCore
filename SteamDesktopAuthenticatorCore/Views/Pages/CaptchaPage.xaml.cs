using System.Windows.Controls;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class CaptchaPage : Page
    {
        public CaptchaPage(CaptchaViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
