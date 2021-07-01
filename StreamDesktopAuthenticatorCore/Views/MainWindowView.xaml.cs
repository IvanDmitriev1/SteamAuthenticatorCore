using System.Drawing;
using System.Windows;
using System.Windows.Input;
using SteamDesktopAuthenticatorCore.Services;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();

            DeactivateAuthenticatorMenuItemImage.Source = BitmapToBitmapImageService.BitmapToBitmapImage(SystemIcons.Warning.ToBitmap());
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void textBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }
    }
}
