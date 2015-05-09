using System;
using System.Windows.Input;

namespace SearchSharp
{
    public class ActionCommand : ICommand
    {
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteFunc;

        public ActionCommand(Action executeAction)
            : this(executeAction, () => true)
        {
        }

        public ActionCommand(Action executeAction, Func<bool> canExecuteFunc)
        {
            _executeAction = executeAction;
            _canExecuteFunc = canExecuteFunc;
        }

        public void Execute(object parameter)
        {
            _executeAction();
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc();
        }

        public event EventHandler CanExecuteChanged;
    }
}
