using System;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WPFUI.DIControls;
using WPFUI.DIControls.Interfaces;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class CaptchaViewModel : ObservableObject, INavigable
{
    public CaptchaViewModel(DefaultNavigation navigation)
    {
        _navigation = navigation;
        _image = new BitmapImage(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/e/eb/Blank.jpg/220px-Blank.jpg", UriKind.Absolute));
    }

    #region Variables

    private readonly DefaultNavigation _navigation;
    private const string? SteamUrl = "https://steamcommunity.com/public/captcha.php?gid=";

    [ObservableProperty]
    private BitmapImage _image;

    [ObservableProperty]
    private string _captchaCode = string.Empty;

    #endregion

    #region Public methods

    public void OnNavigationRequest(INavigation navigation, INavigationItem previousNavigationItem, ref object[]? ars)
    {
        if (ars is null)
            return;

        string captchaUri = SteamUrl + ars[0];
        Image = new BitmapImage(new Uri(captchaUri, UriKind.Absolute));
    }

    #endregion

    #region Commands

    [ICommand]
    private void Submit()
    {
        _navigation.NavigateBack(new object[] {CaptchaCode});
    }

    #endregion
}