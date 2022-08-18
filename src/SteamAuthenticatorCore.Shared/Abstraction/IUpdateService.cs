using SteamAuthenticatorCore.Shared.Models;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IUpdateService
{
    ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground);
    ValueTask<CheckForUpdateModel?> CheckForUpdate(string fileName);
    Task DownloadAndInstall(CheckForUpdateModel updateModel);
}
