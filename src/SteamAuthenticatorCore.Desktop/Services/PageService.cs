﻿using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class PageService : IPageService
{
    public T? GetPage<T>() where T : class
    {
        return App.ServiceProvider.GetService<T>();
    }

    public FrameworkElement? GetPage(Type pageType)
    {
        return App.ServiceProvider.GetRequiredService(pageType) as FrameworkElement;
    }
}