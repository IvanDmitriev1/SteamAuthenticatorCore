using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SteamAuthenticatorCore.Desktop.Helpers;

internal class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _callback;

    public AsyncRelayCommand(Func<Task> func)
    {
        _callback = func;
    }

    private bool _isExecuting;

    private bool IsExecuting
    {
        get => _isExecuting;
        set
        {
            _isExecuting = value;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    public bool CanExecute(object? parameter)
    {
        return !IsExecuting;
    }

    public async void Execute(object? parameter)
    {
        try
        {
            IsExecuting = true;
            await ExecuteAsync();
            IsExecuting = false;

        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            IsExecuting = false;
        }
    }

    private Task ExecuteAsync()
    {
        return _callback.Invoke();
    }

    public event EventHandler? CanExecuteChanged;
}