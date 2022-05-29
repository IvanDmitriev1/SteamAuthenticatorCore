using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorCore.Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MyTitleView : ContentView
	{
		public MyTitleView ()
		{
			InitializeComponent ();
		}

        public static readonly BindableProperty TitleNameProperty =
            BindableProperty.Create (nameof(TitleName), typeof(string), typeof(MyTitleView), string.Empty, BindingMode.OneTime);

        public string TitleName
        {
            get => (string)GetValue (TitleNameProperty);
            set => SetValue (TitleNameProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            label.SetBinding(TitleNameProperty, nameof(TitleName), BindingMode.OneTime);
        }
    }
}