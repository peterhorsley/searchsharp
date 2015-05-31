using System;
using System.Windows;
using System.Windows.Input;

namespace SearchSharp
{
    public abstract class BaseCommand : ICommand
    {
        public abstract void Execute(object parameter);

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                var handler = CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, new EventArgs());
                }
            }));
        }
    }
}
