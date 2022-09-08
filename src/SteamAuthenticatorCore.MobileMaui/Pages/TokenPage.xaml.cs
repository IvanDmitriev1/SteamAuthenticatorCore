using SteamAuthenticatorCore.MobileMaui.ViewModels;
using TokenViewModel = SteamMobileAuthenticator.ViewModels.TokenViewModel;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class TokenPage : ContentPage
{
	public TokenPage(TokenViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}