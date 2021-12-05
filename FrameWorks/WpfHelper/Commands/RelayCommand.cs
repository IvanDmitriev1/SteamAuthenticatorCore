using System;
using WpfHelper.Commands.Base;

namespace WpfHelper.Commands
{
    public class RelayCommand : BaseCommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;
        private readonly Action<Exception>? _onException;

        public RelayCommand(in Action<object?> execute, Func<object?, bool>? canExecute = null, in Action<Exception>? onException = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _onException = onException;
        }

        public override bool CanExecute(object? parameter)
        {
            if (_canExecute is not { } func) return true;

            return func.Invoke(parameter);
        }

        public override void Execute(object? parameter)
        {
            try
            {
                _execute.Invoke(parameter);
            }
            catch (Exception e)
            {
                if (_onException == null)
                    throw new Exception(e.ToString());
                
                _onException.Invoke(e);
            }
        }
    }
}
