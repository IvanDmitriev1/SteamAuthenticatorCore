using System.Windows;
using System.Windows.Input;
using SteamAuthCore;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class WelcomeWindowViewModel : BaseViewModel
    {
        private Window _thisWindow = null!;

        #region Commands

        public ICommand WindowOnLoadedCommand => new RelayCommand( o =>
        {
            if (o is not RoutedEventArgs {Source: Window window}) return;

            _thisWindow = window;
        });

        public ICommand JustRunButtonOnClick => new RelayCommand(o =>
        {
            // Mark as not first run anymore
            ManifestModel manifest = App.ManifestModelService.GetManifestModel();
            manifest.FirstRun = false;
            App.ManifestModelService.SaveManifest();

            ShowMainWindow();
        });

        public ICommand ImportConfigButtonOnClickCommand => new RelayCommand(o =>
        {
            
        });

        #endregion

        private void ShowMainWindow()
        {
            MainWindowView mainWindow = new();
            mainWindow.Show();

            _thisWindow.Close();
        }
    }
}
