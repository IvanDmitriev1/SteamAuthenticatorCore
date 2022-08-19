using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class LoginPage 
{
    public LoginPage(LoginViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
