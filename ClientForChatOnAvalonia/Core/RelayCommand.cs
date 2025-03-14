using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientForChatOnAvalonia
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public event EventHandler CanExecuteChanged;
        public async void Execute(object parameter) => await _executeAsync(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
