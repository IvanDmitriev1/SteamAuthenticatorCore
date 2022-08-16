using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore;
using SteamAuthenticatorCore.Desktop.Messages;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(INavigationService navigation, IMessenger messenger)
    {
        _navigation = navigation;
        messenger.Register<UpdateAccountInLoginPageMessage>(this);
    }

    private readonly INavigationService _navigation;

    [ObservableProperty]
    private SteamGuardAccount _account = null!;

    [RelayCommand]
    public void OnLogin()
    {
        _navigation.NavigateTo("token");
    }

    public void Receive(UpdateAccountInLoginPageMessage message)
    {
        Account = message.Value;
    }
}