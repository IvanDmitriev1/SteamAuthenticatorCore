using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Models;

public abstract class ConfirmationViewModelBase : IConfirmationViewModel
{
    protected ConfirmationViewModelBase(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmations, IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService)
    {
        _platformImplementations = platformImplementations;
        _accountService = accountService;
        Account = account;

        var confirmationModels = confirmations.ToArray();
        foreach (var confirmation in confirmationModels)
        {
            confirmation.BitMapImage = platformImplementations.CreateImage(confirmation.ImageSource);
        }

        Confirmations = new ObservableCollection<ConfirmationModel>(confirmationModels);
    }

    private readonly IPlatformImplementations _platformImplementations;
    private readonly ISteamGuardAccountService _accountService;

    public SteamGuardAccount Account { get; }
    public ObservableCollection<ConfirmationModel> Confirmations { get; }

    public abstract ICommand ConfirmCommand { get; }
    public abstract ICommand CancelCommand { get; }

    public async ValueTask CheckConfirmations()
    {
        Confirmations.Clear();

        foreach (var confirmation in await _accountService.FetchConfirmations(Account))
        {
            confirmation.BitMapImage = _platformImplementations.CreateImage(confirmation.ImageSource);
            Confirmations.Add(confirmation);
        }
    }

    public async Task SendConfirmation(ConfirmationModel confirmation, ConfirmationOptions command)
    {
        if (!await _accountService.SendConfirmation(Account, confirmation, command))
            return;

        await _platformImplementations.InvokeMainThread(() =>
        {
            Confirmations.Remove(confirmation);
        });
    }

    public async Task SendConfirmations(IEnumerable<ConfirmationModel> confirmations, ConfirmationOptions command)
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

        await _accountService.SendConfirmation(Account, confirms, command);

        foreach (var confirmation in confirms)
        {
            await _platformImplementations.InvokeMainThread(() =>
            {
                Confirmations.Remove(confirmation);
            });
        }
    }
}
