using System;
using SteamAuthCore;
using SteamAuthenticatorCore.Mobile.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorCore.Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
	{
		public MainPage ()
		{
            InitializeComponent();
        }

        private void ListView_OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                HapticFeedback.Perform();
            }
            catch
            {
                //
            }

            ((ListView)sender).SelectedItem = null;
        }

        private void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            var context = ((ListView) sender).BindingContext as MainPageViewModel;
            context!.SelectedSteamGuardAccount = e.SelectedItem as SteamGuardAccount; 
        }
    }
}