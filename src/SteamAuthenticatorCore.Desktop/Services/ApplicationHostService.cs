using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SteamAuthenticatorCore.Desktop.Views;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationHostService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var window = _serviceProvider.GetRequiredService<Container>();
        window.Show();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        var window = _serviceProvider.GetRequiredService<Container>();
        window.Close();

        return Task.CompletedTask;
    }
}
