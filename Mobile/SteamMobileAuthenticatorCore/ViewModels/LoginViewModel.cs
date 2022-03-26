using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace SteamAuthenticatorCore.Mobile.ViewModels;

public class LoginViewModel : ObservableObject, IQueryAttributable
{
    public LoginViewModel()
    {
        _accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
        _manifestModelService = DependencyService.Get<IManifestModelService>();

        LoginService = DependencyService.Get<LoginService>();
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private readonly IManifestModelService _manifestModelService;

    public LoginService LoginService { get; }

    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);

        LoginService.Account = _accounts[Convert.ToInt32(id)];
    }

    public ICommand LoginCommand => new SteamAuthenticatorCore.Shared.Helpers.AsyncRelayCommand(async () =>
    {
        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }

        if (LoginService.Account is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await LoginService.RefreshLogin(_manifestModelService);
        await Shell.Current.GoToAsync("..");
    });
}