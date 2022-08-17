using System;
using System.Windows;
using System.Windows.Interop;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;

namespace SteamAuthenticatorCore.Desktop.Services;

public sealed class TaskBarServiceWrapper
{
    public TaskBarServiceWrapper(ITaskBarService taskBarService)
    {
        _taskBarService = taskBarService;
    }

    private readonly ITaskBarService _taskBarService;
    private IntPtr _windowHandle;

    public void SetActiveWindow(Window window)
    {
        _windowHandle = new WindowInteropHelper(window).Handle;
    }

    public void SetState(TaskBarProgressState state)
    {
        var q = _taskBarService.SetState(_windowHandle, state);
    }
}
