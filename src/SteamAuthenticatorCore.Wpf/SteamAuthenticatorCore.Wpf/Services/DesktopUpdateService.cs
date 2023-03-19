using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Octokit;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopUpdateService : UpdateServiceBase
{
    public DesktopUpdateService(ILogger<DesktopUpdateService> logger) : base(Assembly.GetExecutingAssembly().GetName().Version!)
    {
        _logger = logger;
    }

    private readonly ILogger<DesktopUpdateService> _logger;

    public override async Task DownloadAndInstall(Release release)
    {
        /*const string newFileName = "new.exe";
        var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), newFileName);
        await using (var stream = await Client.GetStreamAsync(updateModel.DownloadUrl))
        {
            await using var fileStream = new FileStream(newFilePath, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
        }
        var currentExeName = AppDomain.CurrentDomain.FriendlyName + ".exe";
        var currentExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentExeName);
        var commandLineBuilder = new StringBuilder();
        commandLineBuilder.Append($"taskkill /f /im \"{currentExeName}\"");
        commandLineBuilder.Append(" && ");
        commandLineBuilder.Append("timeout /t 1");
        commandLineBuilder.Append(" && ");
        commandLineBuilder.Append($"del \"{currentExePath}\"");
        commandLineBuilder.Append(" && ");
        commandLineBuilder.Append($"ren \"{newFileName}\" \"{currentExeName}\"");
        commandLineBuilder.Append(" && ");
        commandLineBuilder.Append($"\"{currentExePath}\"");
        Cmd(commandLineBuilder.ToString());*/
    }

    private static void Cmd(string line)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c {line}",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }
}