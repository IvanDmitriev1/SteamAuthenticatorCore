using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class PageService : IPageService
{
    public PageService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private readonly IServiceProvider _serviceProvider;

    public T? GetPage<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }

    public FrameworkElement? GetPage(Type pageType)
    {
        return _serviceProvider.GetRequiredService(pageType) as FrameworkElement;
    }
}
