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

    public Task SendConfirmation(ConfirmationModel confirmation, SteamGuardAccount.Confirmation command) =>
        Task.Run(() =>
        {
            Account.SendConfirmationAjax(confirmation, command);

            _platformImplementations.InvokeMainThread(() =>
            {
                Confirmations.Remove(confirmation);
            });
        });

    public Task SendConfirmations(IEnumerable<ConfirmationModel> confirmations, SteamGuardAccount.Confirmation command) =>
        Task.Run(() =>
        {
            var confirmationModels = confirmations as ConfirmationModel[] ?? confirmations.ToArray();
            SendConfirmations(confirmationModels, command);
        });

    public void SendConfirmations(IReadOnlyCollection<ConfirmationModel> confirmations, SteamGuardAccount.Confirmation command)
    {
        Account.SendConfirmationAjax(confirmations, command);

        foreach (var confirmation in confirmations)
        {
            _platformImplementations.InvokeMainThread(() =>
            {
                Confirmations.Remove(confirmation);
            });
        }
    }
}
