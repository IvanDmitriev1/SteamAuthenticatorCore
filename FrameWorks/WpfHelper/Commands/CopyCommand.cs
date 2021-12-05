using System;
using System.Diagnostics;
using System.Windows;
using WpfHelper.Commands.Base;

namespace WpfHelper.Commands
{
    public class CopyCommand : BaseCommand
    {
        public override bool CanExecute(object? parameter)
        {
            return true;
        }

        public override void Execute(object? parameter)
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
                Execute(parameter);
            }
        }
    }
}