using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IConfirmationService
{
    ObservableCollection<IConfirmationViewModel> ConfirmationViewModels { get; }

    void Initialize();
    ValueTask CheckConfirmations();
}
