using SteamAuthenticatorCore.MobileMaui.ViewModels;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}