using System;
using System.Windows.Input;

namespace NetworkService.Helpers
{
    public class MyICommandWithParameter<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public MyICommandWithParameter(Action<T> execute)
        {
            this.execute = execute;
        }

        public MyICommandWithParameter(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
            {
                return true;
            }

            return canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}