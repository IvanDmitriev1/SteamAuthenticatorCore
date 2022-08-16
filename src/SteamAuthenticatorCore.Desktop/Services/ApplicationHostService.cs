using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using SteamAuthenticatorCore.Desktop.Views;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class ApplicationHostService : IHostedService
{
    public ApplicationHostService(IHub hub)
    {
        _hub = hub;
    }

    private readonly IHub _hub;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<Container>();
            window.Show();

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            App.OnException(e, _hub);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<Container>();
            window.Close();
        }
        catch (Exception e)
        {
            App.OnException(e, _hub);
        }

        return Task.CompletedTask;
    }
}
