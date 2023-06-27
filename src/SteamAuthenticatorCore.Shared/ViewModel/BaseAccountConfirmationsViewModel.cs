namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class BaseAccountConfirmationsViewModel : MyObservableRecipient, IRecipient<UpdateAccountConfirmationPageMessage>
{
    protected BaseAccountConfirmationsViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations)
    {
        _accountService = accountService;
        _platformImplementations = platformImplementations;
    }

    private readonly ISteamGuardAccountService _accountService;
    private readonly IPlatformImplementations _platformImplementations;

    [ObservableProperty]
    private SteamGuardAccountConfirmationsModel? _model;

    public void Receive(UpdateAccountConfirmationPageMessage message)
    {
        Model = message.Value;
    }

    protected async ValueTask SendConfirmations(IEnumerable<Confirmation> confirmations, ConfirmationOptions command)
    {
        IReadOnlyList<Confirmation> confirms = confirmations as List<Confirmation> ?? confirmations.ToList();

        if (confirms.Count == 0)
            return;
        /*
         if (confirms.Length == 1)
        {
            if (await _accountService.SendConfirmation(Model!.Account, confirms[0], command, CancellationToken.None))
            {
                _platformImplementations.InvokeMainThread(() =>
                {
                    Model.Confirmations.Remove(confirms[0]);
                });
            }

            return;
        }
         */

        if (!await _accountService.SendConfirmation(Model!.Account, confirms, command, CancellationToken.None))
            return;

        _platformImplementations.InvokeMainThread(() =>
        {
            Model.Confirmations.Clear();
        });
    }
}
