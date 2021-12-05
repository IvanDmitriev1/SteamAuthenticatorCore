using System;
using System.Threading.Tasks;
using WpfHelper.Commands.Base;

namespace WpfHelper.Commands
{
    public class AsyncRelayCommand : AsyncCommandBase
    {
        private readonly Func<object?, Task> _callback;
        private readonly Func<object?, bool>? _canExecute;

        public AsyncRelayCommand(in Func<object?, Task> callback,in Func<object?, bool>? canExecute = null, in Action<Exception>? onException = null) : base(onException)
        {
            _callback = callback;
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter)
        {
            if (_canExecute is not { } func) return !IsExecuting;

            if (func.Invoke(parameter))
                return !IsExecuting;

            return false;
        }

        protected override async Task ExecuteAsync(object? parameter)
        {
            await _callback(parameter);
        }
    }
}
