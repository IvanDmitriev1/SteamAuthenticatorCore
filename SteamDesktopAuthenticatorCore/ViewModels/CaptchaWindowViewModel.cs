using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class CaptchaWindowViewModel : BaseViewModel
    {
        public CaptchaWindowViewModel()
        {
            _image = new BitmapImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/e/eb/Blank.jpg/220px-Blank.jpg", UriKind.Absolute));
        }

        #region Variables

        private Window? _thisWindow;

        private BitmapImage _image;
        private const string? SteamUrl = "https://steamcommunity.com/public/captcha.php?gid=";
        private string? _captchaUrl;
        private string? _captchaGid;
        private string _captchaCode = string.Empty;

        #endregion

        #region Fields

        public string CaptchaCode
        {
            get => _captchaCode;
            set => Set(ref _captchaCode, value);
        }

        public string? CaptchaGid
        {
            get => _captchaGid;
            set
            {
                _captchaUrl = SteamUrl + value;
                Image = new BitmapImage(new Uri(_captchaUrl, UriKind.Absolute));

                Set(ref _captchaGid, value, nameof(CaptchaGid), nameof(Image));
            }
        }

        public BitmapImage Image
        {
            get => _image;
            private set => Set(ref _image, value);
        }

        #endregion

        #region Commands
        public ICommand WindowOnLoadedCommand => new RelayCommand(o =>
        {
            if (o is not RoutedEventArgs { Source: Window window }) return;

            _thisWindow = window;
        });

        public ICommand SubmitCommand => new RelayCommand(o =>
        {
            _thisWindow?.Close();
        });

        #endregion


    }
}
