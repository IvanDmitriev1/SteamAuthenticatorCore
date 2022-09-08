using ConfirmationViewModel = SteamAuthenticatorCore.Mobile.ViewModels.ConfirmationViewModel;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class ConfirmationsPage : ContentPage
{
	public ConfirmationsPage(ConfirmationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}