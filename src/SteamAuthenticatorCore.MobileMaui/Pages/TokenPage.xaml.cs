using SteamAuthenticatorCore.MobileMaui.ViewModels;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class TokenPage : ContentPage
{
	public TokenPage(TokenViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}