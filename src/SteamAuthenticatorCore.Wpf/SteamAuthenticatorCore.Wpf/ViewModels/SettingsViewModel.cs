using System.Reflection;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class SettingsViewModel : SettingsViewModelBase
{
    public SettingsViewModel(AppSettings appSettings, IUpdateService updateService) : base(appSettings, updateService, Assembly.GetExecutingAssembly().GetName().Version!.ToString())
    {
    }
}
