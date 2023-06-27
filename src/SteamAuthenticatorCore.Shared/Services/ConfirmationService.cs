namespace SteamAuthenticatorCore.Shared.Services;

internal sealed class ConfirmationService : IConfirmationService, IAsyncDisposable
{
    public ConfirmationService(IBackgroundTimerFactory backgroundTimerFactory, ISteamGuardAccountService steamAccountService, IPlatformImplementations platformImplementations, AccountsServiceResolver accountsServiceResolver)
    {
        _steamAccountService = steamAccountService;
        _platformImplementations = platformImplementations;
        _accountsServiceResolver = accountsServiceResolver;

        AppSettings.Current.PropertyChanged += SettingsOnPropertyChanged;

        _backgroundTimer = backgroundTimerFactory.InitializeTimer(TradeAutoConfirmationTimerOnTick);
    }

    private readonly ISteamGuardAccountService _steamAccountService;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly AccountsServiceResolver _accountsServiceResolver;
    private readonly IBackgroundTimer _backgroundTimer;

    private IAccountsService _accountsService = null!;

    public async Task Initialize()
    {
        _accountsService = _accountsServiceResolver.Invoke();

        if (!AppSettings.Current.AutoConfirmMarketTransactions)
            return;

        await _backgroundTimer.StartOrRestart(TimeSpan.FromSeconds(AppSettings.Current.PeriodicCheckingInterval));
    }

    public async ValueTask DisposeAsync()
    {
        await _backgroundTimer.DisposeAsync();

        AppSettings.Current.PropertyChanged -= SettingsOnPropertyChanged;
    }

    public async Task<IReadOnlyList<SteamGuardAccountConfirmationsModel>> CheckConfirmationFromAllAccounts()
    {
        var accountConfirmations = new List<SteamGuardAccountConfirmationsModel>();

        foreach (var account in await _accountsService.GetAll())
        {
            await Task.Delay(TimeSpan.FromSeconds(1.5));

            IReadOnlyList<Confirmation> confirmations;
            try
            {
                confirmations = await _steamAccountService.FetchConfirmations(account, CancellationToken.None);
            }
            catch (Exception e)
            {
                continue;
                //TODO
            }

            if (!confirmations.Any())
                continue;

            foreach (var confirmation in confirmations)
            {
                confirmation.BitMapIcon = _platformImplementations.CreateImage(confirmation.Icon);
            }

            accountConfirmations.Add(new SteamGuardAccountConfirmationsModel(account, confirmations));
        }

        return accountConfirmations;
    }

    private async Task TradeAutoConfirmationTimerOnTick(CancellationToken cancellationToken)
    {
        foreach (var account in await _accountsService.GetAll())
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            var confirmations = await _steamAccountService.FetchConfirmations(account, cancellationToken);
            var sortedConfirmations = confirmations.Where(model => model.Type == ConfirmationType.MarketSellTransaction).ToList();

            if (sortedConfirmations.Count == 0)
                continue;

            await _steamAccountService.SendConfirmation(account, sortedConfirmations, ConfirmationOptions.Allow, cancellationToken);
        }
    }

    private async void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsLoaded)
            return;

        if (e.PropertyName == nameof(settings.AccountsLocation))
        {
            _accountsService = _accountsServiceResolver.Invoke();
            return;
        }

        if (e.PropertyName == nameof(settings.PeriodicCheckingInterval))
        {
            if (!settings.AutoConfirmMarketTransactions)
                return;

            await _backgroundTimer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval));
            return;
        }

        if (e.PropertyName != nameof(settings.AutoConfirmMarketTransactions))
            return;

        if (settings.AutoConfirmMarketTransactions)
        {
            await _backgroundTimer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval));
        }
        else
        {
            await _backgroundTimer.Stop();
        }
    }
}
