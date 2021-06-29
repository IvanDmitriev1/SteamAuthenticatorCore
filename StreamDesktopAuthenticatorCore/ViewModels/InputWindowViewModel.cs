using System;
using System.Windows;
using System.Windows.Input;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class InputWindowViewModel : BaseViewModel
    {
        #region Variabels

        private Window? _thisWindow;

        private string _text = string.Empty;
        private string _inputText = string.Empty;

        #endregion

        #region Fields

        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }

        public string InputString
        {
            get => _inputText;
            set => Set(ref _inputText, value);
        }

        #endregion

        #region Commands

        public ICommand WindowOnLoadedCommand => new RelayCommand(o =>
        {
            if (o is not RoutedEventArgs { Source: Window window }) return;

            _thisWindow = window;
        });

        public ICommand ButtonClickedCommand => new RelayCommand(o =>
        {
            if (string.IsNullOrEmpty(InputString))
                return;

            _thisWindow?.Close();
        });

        #endregion
    }
}
