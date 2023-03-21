using SteamAuthCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthCore.Abstractions;
using SteamAuthenticatorCore.Shared.Abstractions;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Messages;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class ConfirmationsViewModelBase : ObservableObject, IRecipient<UpdateAccountConfirmationPageMessage>
{
    protected ConfirmationsViewModelBase(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations, IMessenger messenger)
    {
        _accountService = accountService;
        _platformImplementations = platformImplementations;

        messenger.Register(this);
    }

    private readonly ISteamGuardAccountService _accountService;
    private readonly IPlatformImplementations _platformImplementations;

    [ObservableProperty]
    private Models.SteamGuardAccountConfirmationsModel _steamGuardAccountConfirmationsModel = null!;

    public void Receive(UpdateAccountConfirmationPageMessage message)
    {
        SteamGuardAccountConfirmationsModel = message.Value;
    }

    protected async ValueTask SendConfirmation(ConfirmationModel confirmation, ConfirmationOptions command)
    {
        await _accountService.SendConfirmation(_steamGuardAccountConfirmationsModel.Account, confirmation, command, CancellationToken.None);

        _steamGuardAccountConfirmationsModel.Confirmations.Remove(confirmation);
    }

    protected async ValueTask SendConfirmations(IEnumerable<ConfirmationModel> confirmations, ConfirmationOptions command)
    {
        var confirms = confirmations.ToArray();

        switch (confirms.Length)
        {
            case 0:
                return;
            case 1:
                await SendConfirmation(confirms[0], command);
                return;
        }

        await _accountService.SendConfirmation(_steamGuardAccountConfirmationsModel.Account, confirms, command, CancellationToken.None);

        foreach (var confirmation in confirms)
        {
            _steamGuardAccountConfirmationsModel.Confirmations.Remove(confirmation);
        }
    }
}
