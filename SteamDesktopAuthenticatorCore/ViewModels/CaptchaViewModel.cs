using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WPFUI.Common;
using WPFUI.Controls.Navigation;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class CaptchaViewModel : BaseViewModel, INavigable
    {
        public CaptchaViewModel(DefaultNavigation navigation)
        {
            _navigation = navigation;
            _image = new BitmapImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/e/eb/Blank.jpg/220px-Blank.jpg", UriKind.Absolute));
        }

        #region Variables

        private readonly DefaultNavigation _navigation;
        private const string? SteamUrl = "https://steamcommunity.com/public/captcha.php?gid=";

        private BitmapImage _image;
        private string _captchaCode = string.Empty;

        #endregion

        #region Public properties

        public BitmapImage Image
        {
            get => _image;
            set => Set(ref _image, value);
        }

        public string CaptchaCode
        {
            get => _captchaCode;
            set => Set(ref _captchaCode, value);
        }

        #endregion

        #region Public methods

        public bool OnNavigationRequest(INavigation navigation, object[]? ars)
        {
            if (ars is null)
                return false;

            string captchaUri = SteamUrl + ars[0];
            Image = new BitmapImage(new Uri(captchaUri, UriKind.Absolute));

            return true;
        }

        #endregion

        #region Commands

        public ICommand SubmitCommand => new RelayCommand(o =>
        {
            _navigation.NavigateBack(new object[] {CaptchaCode});
        });

        #endregion
    }
}
