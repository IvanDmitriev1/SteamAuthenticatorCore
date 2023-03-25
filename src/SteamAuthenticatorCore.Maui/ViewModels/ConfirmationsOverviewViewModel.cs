using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class ConfirmationsOverviewViewModel : ObservableRecipient
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase)
    {
        _confirmationServiceBase = confirmationServiceBase;
    }

    private readonly IConfirmationService _confirmationServiceBase;
    private bool _needRefresh;

    protected override void OnActivated()
    {
        base.OnActivated();


    }
}