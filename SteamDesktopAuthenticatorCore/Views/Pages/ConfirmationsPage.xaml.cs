using System.Windows.Controls;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views.Pages
{
    public partial class ConfirmationsPage : Page
    {
        public ConfirmationsPage(ConfirmationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
