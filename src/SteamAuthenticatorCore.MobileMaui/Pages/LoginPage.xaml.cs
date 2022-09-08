using SteamAuthenticatorCore.MobileMaui.ViewModels;
using LoginViewModel = SteamMobileAuthenticator.ViewModels.LoginViewModel;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}