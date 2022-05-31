using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorCore.Mobile.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MyTitleView : ContentView
	{
		public MyTitleView()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TitleNameProperty =
            BindableProperty.Create(nameof(TitleName), typeof(string), typeof(MyTitleView), string.Empty, BindingMode.OneTime);

        public static readonly BindableProperty IsContentVisibleProperty =
            BindableProperty.Create(nameof(IsContentVisible), typeof(bool), typeof(MyTitleView), false);

        public static readonly BindableProperty IsCloseButtonVisibleProperty =
            BindableProperty.Create(nameof(IsCloseButtonVisible), typeof(bool), typeof(MyTitleView), true);
        
        public static readonly BindableProperty CloseCommandProperty = 
            BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(MyTitleView), null, BindingMode.OneTime);

        public string TitleName
        {
            get => (string)GetValue (TitleNameProperty);
            set => SetValue (TitleNameProperty, value);
        }

        public bool IsContentVisible
        {
            get => (bool)GetValue (IsContentVisibleProperty);
            set => SetValue (IsContentVisibleProperty, value);
        }

        public bool IsCloseButtonVisible
        {
            get => (bool)GetValue (IsCloseButtonVisibleProperty);
            set => SetValue (IsCloseButtonVisibleProperty, value);
        }

        public ICommand? CloseCommand
        {
            get => (ICommand)GetValue (CloseCommandProperty);
            set => SetValue (CloseCommandProperty, value);
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            CloseCommand?.Execute(null);
        }
    }
}