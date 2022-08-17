using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.Models;

public abstract class ConfirmationAccountModelBase
{
    public ConfirmationAccountModelBase(SteamGuardAccount account, ConfirmationModel[] confirmations,
        IPlatformImplementations platformImplementations)
    {
        _platformImplementations = platformImplementations;
        Account = account;

        foreach (var confirmation in confirmations)
            confirmation.BitMapImage = platformImplementations.CreateImage(confirmation.ImageSource);

        Confirmations = new ObservableCollection<ConfirmationModel>(confirmations);
    }

    private readonly IPlatformImplementations _platformImplementations;

    public SteamGuardAccount Account { get; }
    public ObservableCollection<ConfirmationModel> Confirmations { get; }

    public abstract ICommand ConfirmCommand { get; }
    public abstract ICommand CancelCommand { get; }

    public async ValueTask CheckConfirmations()
    {
        Confirmations.Clear();

        var confirmations = await TryGetConfirmations(Account);

        foreach (var confirmation in confirmations)
        {
            Confirmations.Add(confirmation);
        }
    }

    public async Task SendConfirmation(ConfirmationModel confirmation, SteamGuardAccount.Confirmation command)
    {
        Account.SendConfirmationAjax(confirmation, command);

        await _platformImplementations.InvokeMainThread(() =>
        {
            Confirmations.Remove(confirmation);
        });
    }

    public Task SendConfirmations(IEnumerable<ConfirmationModel> confirmations, SteamGuardAccount.Confirmation command)
    {
        var confirmationModels = confirmations as ConfirmationModel[] ?? confirmations.ToArray();
        return SendConfirmations(confirmationModels, command);
    }

    public async Task SendConfirmations(IReadOnlyCollection<ConfirmationModel> confirmations, SteamGuardAccount.Confirmation command)
    {
        Account.SendConfirmationAjax(confirmations, command);

        foreach (var confirmation in confirmations)
        {
            await _platformImplementations.InvokeMainThread(() =>
            {
                Confirmations.Remove(confirmation);
            });
        }
    }

    public static async ValueTask<ConfirmationModel[]> TryGetConfirmations(SteamGuardAccount account)
    {
        try
        {
            return (await account.FetchConfirmationsAsync().ConfigureAwait(false)).ToArray();
        }
        catch (SteamGuardAccount.WgTokenInvalidException)
        {
            await account.RefreshSessionAsync();

            try
            {
                return (await account.FetchConfirmationsAsync().ConfigureAwait(false)).ToArray();
            }
            catch (SteamGuardAccount.WgTokenInvalidException)
            {
            }
        }
        catch (SteamGuardAccount.WgTokenExpiredException)
        {

        }

        return Array.Empty<ConfirmationModel>();
    }
}
