using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IConfirmationService
{
    ObservableCollection<ConfirmationAccountModelBase> Accounts { get; }

    void Initialize();
    ValueTask CheckConfirmations();
}
