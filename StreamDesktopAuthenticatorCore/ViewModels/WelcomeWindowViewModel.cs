using System.Windows;
using System.Windows.Input;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class WelcomeWindowViewModel : BaseViewModel
    {
        public WelcomeWindowViewModel()
        {

        }

        private ManifestModel _manifest = null!;

        private Window _thisWindow = null!;

        #region Commands

        public ICommand WindowOnLoadedCommand => new RelayCommand(async o =>
        {
            if (o is not RoutedEventArgs {Source: Window window}) return;

            _thisWindow = window;
            _manifest = await ManifestModelService.GetManifestFromGoogleDrive();
        });

        public ICommand JustRunButtonOnClick => new AsyncRelayCommand(async o =>
        {
            // Mark as not first run anymore
            _manifest.FirstRun = false;
            await ManifestModelService.SaveManifestInGoogleFile();

            ShowMainWindow();
        });

        #endregion

        private void ShowMainWindow()
        {
            MainWindowView mainWindow = new();
            var mainWindowDataContext = (mainWindow.DataContext as MainWindowViewModel)!;
            mainWindowDataContext.Manifest = _manifest; //-V3149
            mainWindow.Show();

            _thisWindow.Close();
        }
    }
}
