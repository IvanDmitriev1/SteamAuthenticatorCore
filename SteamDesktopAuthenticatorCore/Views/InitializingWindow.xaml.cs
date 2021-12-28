using System.Windows;
using SteamDesktopAuthenticatorCore.ViewModels;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class InitializingWindow : Window
    {
        public InitializingWindow(InitializingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
