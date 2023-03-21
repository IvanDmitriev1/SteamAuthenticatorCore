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

public abstract partial class ConfirmationsViewModelBase : ObservableRecipient, IRecipient<UpdateAccountConfirmationPageMessage>
{
    protected ConfirmationsViewModelBase(ISteamGuardAccountService accountService)
    {
        _accountService = accountService;
    }

    private readonly ISteamGuardAccountService _accountService;

    [ObservableProperty]
    private SteamGuardAccountConfirmationsModel? _steamGuardAccountConfirmationsModel;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    public void Receive(UpdateAccountConfirmationPageMessage message)
    {
        SteamGuardAccountConfirmationsModel = message.Value;
        PageTitle = $"{SteamGuardAccountConfirmationsModel.Account.AccountName} confirmations";
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
