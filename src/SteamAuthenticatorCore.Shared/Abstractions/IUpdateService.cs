using System.Threading.Tasks;
using Octokit;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IUpdateService
{
    Task<Release?> CheckForUpdate();
    Task DownloadAndInstall(Release release);
}
