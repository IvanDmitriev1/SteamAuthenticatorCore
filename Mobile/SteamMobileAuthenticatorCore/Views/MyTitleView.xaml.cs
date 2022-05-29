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
            BindableProperty.Create(nameof(TitleName), typeof(string), typeof(MyTitleView), string.Empty,
                BindingMode.OneTime, propertyChanged: TitleNamePropertyChanged);

        public string TitleName
        {
            get => (string)GetValue (TitleNameProperty);
            set => SetValue (TitleNameProperty, value);
        }

        private static void TitleNamePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var control = (MyTitleView)bindable;
            control.label.Text = (string) newvalue;
        }
    }
}