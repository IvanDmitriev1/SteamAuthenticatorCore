using System.Windows.Controls;
using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages
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
