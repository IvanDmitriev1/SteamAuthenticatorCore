using System.Windows.Input;
using WpfHelper.Commands;
using WpfHelper.Common;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    internal class InitializingViewModel : BaseViewModel
    {
        public ICommand WindowLoadedCommand => new RelayCommand(o =>
        {

        });
    }
}
