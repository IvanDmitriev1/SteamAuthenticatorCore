using SteamAuthenticatorCore.MobileMaui.ViewModels;
using ConfirmationsOverviewViewModel = SteamMobileAuthenticator.ViewModels.ConfirmationsOverviewViewModel;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class ConfirmationsOverviewPage : ContentPage
{
	public ConfirmationsOverviewPage(ConfirmationsOverviewViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}