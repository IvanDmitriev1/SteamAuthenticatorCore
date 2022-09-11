using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IConfirmationService
{
    ObservableCollection<ConfirmationModel> ConfirmationViewModels { get; }

    void Initialize();
    Task CheckConfirmations();
}
