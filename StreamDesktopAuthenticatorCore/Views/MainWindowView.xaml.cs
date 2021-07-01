using System.Drawing;
using System.Windows;
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
    }
}
