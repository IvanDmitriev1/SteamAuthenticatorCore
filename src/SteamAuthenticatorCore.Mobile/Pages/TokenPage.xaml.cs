using TokenViewModel = SteamAuthenticatorCore.Mobile.ViewModels.TokenViewModel;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class TokenPage : ContentPage
{
	public TokenPage(TokenViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}