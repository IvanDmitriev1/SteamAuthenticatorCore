using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class ConfirmationAccountModel : ConfirmationAccountBase
    {
        public ConfirmationAccountModel(SteamGuardAccount account, ConfirmationModel[] confirmations,
            IPlatformImplementations platformImplementations) : base(account, confirmations, platformImplementations) { }

        public override ICommand ConfirmCommand => new Command<object>( o =>
        {
            Task.Run(() =>
            {
                var list = (IList)o!;
                var confirmations = list.OfType<ConfirmationModel>();

                SendConfirmations(ref confirmations, SteamGuardAccount.Confirmation.Allow);
            });
        });

        public override ICommand CancelCommand => new AsyncCommand<object>( async o =>
        {
            await Task.Run(() =>
            {
                var list = (IList)o!;
                var confirmations = list.OfType<ConfirmationModel>();

                SendConfirmations(ref confirmations, SteamGuardAccount.Confirmation.Deny);
            });
        });
    }

internal class MobileConfirmationService : BaseConfirmationService
{
    public MobileConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, IPlatformImplementations platformImplementations, IPlatformTimer timer) : base(steamGuardAccounts, settings, platformImplementations, timer)
    {
    }

    protected override async Task<ConfirmationAccountBase?> CreateConfirmationAccountViewModel(SteamGuardAccount account)
    {
        var confirmations = (await account.FetchConfirmationsAsync()).ToArray();
        return confirmations.Length > 0 ? new ConfirmationAccountModel(account, confirmations, PlatformImplementations) : null;
    }
}