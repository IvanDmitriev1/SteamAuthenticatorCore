using System.Windows.Input;
using SteamAuthCore.Manifest;
using SteamDesktopAuthenticatorCore.Common;
using WpfHelper.Commands;
using WpfHelper.Common;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class InitializingViewModel : BaseViewModel
    {
        public InitializingViewModel(SettingService settingService, App.ManifestServiceResolver manifestServiceResolver)
        {
            _manifestModelService = manifestServiceResolver.Invoke();
            _appSettings = settingService.Get<AppSettings>();
        }

        private readonly IManifestModelService _manifestModelService;
        private readonly AppSettings _appSettings;

        public ICommand WindowLoadedCommand => new AsyncRelayCommand(async o =>
        {
            await _manifestModelService.Initialize();
        });
    }
}
