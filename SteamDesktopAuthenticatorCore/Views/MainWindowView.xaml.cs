using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();

            BitmapImage warningImage = BitmapToBitmapImageConvertService.BitmapToBitmapImage(SystemIcons.Warning.ToBitmap());

            DeactivateAuthenticatorMenuItemImage.Source = warningImage;
            SingleDropDownMenuItemImage.Source = warningImage;
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
