using System.Diagnostics;
using System.IO;
using System;
using Sentry;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Desktop.Extensions;

internal static class UpdateServiceExtensions
{
    public static void DeletePreviousFile(this IUpdateService updateService, string appFileName)
    {
        var files = Directory.GetFiles(Environment.CurrentDirectory);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
    
            if (!fileName.Contains(appFileName) || !fileName.Contains("exe")) 
                continue;

            if (fileName == Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName))
                continue;
    
            try
            {
                File.Delete(file);
            }
            catch(Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        }
    }
}