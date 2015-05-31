using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchSharp.ViewModels
{
    public class DelegateCommand : BaseCommand
    {
        private readonly Action _command;
        private readonly Func<bool> _canExecute;

        public DelegateCommand(Action command, Func<bool> canExecute)
        {
            _command = command;
            _canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            _command();
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute();
        }
    }
}
