using SteamMobileAuthenticatorCore.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}