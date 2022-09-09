using SteamAuthenticatorCore.Mobile.ViewModels;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class ConfirmationsPage : ContentPage
{
	public ConfirmationsPage(ConfirmationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}