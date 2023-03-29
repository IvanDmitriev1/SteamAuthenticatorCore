using SteamAuthCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthCore.Abstractions;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class BaseAccountConfirmationsViewModel : ObservableRecipient, IRecipient<UpdateAccountConfirmationPageMessage>
{
    protected BaseAccountConfirmationsViewModel(ISteamGuardAccountService accountService)
    {
        _accountService = accountService;

        Messenger.RegisterAll(this);
    }

    private readonly ISteamGuardAccountService _accountService;

    [ObservableProperty]
    private SteamGuardAccountConfirmationsModel? _model;

    protected override void OnActivated()
    {
        
    }

    public void Receive(UpdateAccountConfirmationPageMessage message)
    {
        Model = message.Value;
    }

    protected async ValueTask SendConfirmations(IEnumerable<ConfirmationModel> confirmations, ConfirmationOptions command)
    {
        var confirms = confirmations as ConfirmationModel[] ?? confirmations.ToArray();

        if (confirms.Length == 0)
            return;

        if (confirms.Length == 1)
        {
            if (await _accountService.SendConfirmation(Model!.Account, confirms[0], command, CancellationToken.None))
                Model.Confirmations.Remove(confirms[0]);

            return;
        }

        if (!await _accountService.SendConfirmation(Model!.Account, confirms, command, CancellationToken.None))
            return;

        Model.Confirmations.Clear();
    }
}
