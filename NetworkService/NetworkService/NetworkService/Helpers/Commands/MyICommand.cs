using System;
using System.Windows.Input;

namespace NetworkService.Helpers
{
    public class MyICommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public MyICommand(Action execute)
        {
            this.execute = execute;
        }

        public MyICommand(Action execute, Func<bool> canExecute)
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

            return canExecute();
        }

        public void Execute(object parameter)
        {
            execute();
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