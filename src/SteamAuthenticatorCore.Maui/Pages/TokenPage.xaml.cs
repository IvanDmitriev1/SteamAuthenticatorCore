using SteamAuthenticatorCore.Mobile.ViewModels;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class TokenPage : ContentPage
{
	public TokenPage(TokenViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}