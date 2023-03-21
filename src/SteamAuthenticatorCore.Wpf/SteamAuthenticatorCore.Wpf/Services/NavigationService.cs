using System;
using System.Diagnostics;
using Wpf.Ui.Controls.Navigation;

namespace SteamAuthenticatorCore.Desktop.Services;

public sealed class NavigationService
{
    public static NavigationService Default { get; } = new();

    private INavigationView? _navigationView;

    public void SetNavigationControl(INavigationView navigation)
    {
        _navigationView = navigation;
    }

    public INavigationView GetNavigationView()
    {
        Debug.Assert(_navigationView != null, nameof(_navigationView) + " != null");
        return _navigationView;
    }

    public void Navigate(Type pageType)
    {
        _navigationView?.Navigate(pageType);
    }

    public void Navigate(string pageIdOrTargetTag)
    {
        _navigationView?.Navigate(pageIdOrTargetTag);
    }

    public void GoBack()
    {
        _navigationView?.GoBack();
    }

    public void NavigateWithHierarchy(Type pageType)
    {
        _navigationView?.NavigateWithHierarchy(pageType);
    }
}