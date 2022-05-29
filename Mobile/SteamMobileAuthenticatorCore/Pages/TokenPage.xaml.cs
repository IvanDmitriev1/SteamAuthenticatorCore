using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Mobile.Helpers;
using SteamAuthenticatorCore.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorCore.Mobile.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TokenPage : ContentPage, IBackButtonAction
	{
		public TokenPage ()
		{
            InitializeComponent();
            OnBackActionAsync = OnBackActionAsyncFunc;
        }

        private async Task<bool> OnBackActionAsyncFunc()
        {
            var viewModel = (TokenPageViewModel) BindingContext;

            if (!viewModel.IsLongPressTitleViewVisible) return false;

            await viewModel.UnselectLongPressFrame();
            return true;
        }

        public Func<Task<bool>>? OnBackActionAsync { get; set; }
    }
}