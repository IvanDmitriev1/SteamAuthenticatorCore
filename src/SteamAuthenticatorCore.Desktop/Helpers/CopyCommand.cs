using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace SteamAuthenticatorCore.Desktop.Helpers;

internal class CopyCommand : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        if (parameter is not string text) return;

        try
        {
            Clipboard.Clear();
            Clipboard.SetText(text);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }
}