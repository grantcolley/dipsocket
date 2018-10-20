using System;
using System.Windows.Input;

namespace Client.Command
{
    public class ViewModelCommand : ICommand
    {
        private readonly Predicate<object> canExecute;
        private readonly Action<object> execute;

        public ViewModelCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public ViewModelCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            var canExecuteChanged = CanExecuteChanged;
            if (canExecuteChanged != null)
            {
                canExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
