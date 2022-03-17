using System.Windows.Controls;
using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class LoginPage : Page
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}