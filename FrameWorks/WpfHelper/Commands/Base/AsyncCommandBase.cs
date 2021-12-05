using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfHelper.Commands.Base
{
    public abstract class AsyncCommandBase : ICommand
    {
        private readonly Action<Exception>? _onException;
        private bool _isExecuting;

        protected AsyncCommandBase(in Action<Exception>? onException = null)
        {
            _onException = onException;
        }

        public event EventHandler? CanExecuteChanged;

        protected bool IsExecuting
        {
            get => _isExecuting;
            set
            {
                _isExecuting = value;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        public abstract bool CanExecute(object? parameter);

        public async void Execute(object? parameter)
        {
            try
            {
                IsExecuting = true;
                await ExecuteAsync(parameter);
                IsExecuting = false;

            }
            catch (Exception e)
            {
                if (_onException == null)
                    throw;

                _onException.Invoke(e);
                IsExecuting = false;
            }
        }

        protected abstract Task ExecuteAsync(object? parameter);
    }
}
