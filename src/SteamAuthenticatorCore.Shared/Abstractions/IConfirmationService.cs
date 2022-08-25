using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IConfirmationService
{
    ObservableCollection<IConfirmationViewModel> ConfirmationViewModels { get; }

    void Initialize();
    ValueTask CheckConfirmations();
}
