using SteamAuthenticatorCore.Mobile.ViewModels;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}