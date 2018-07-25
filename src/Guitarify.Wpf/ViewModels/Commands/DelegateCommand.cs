using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Guitarify.Wpf.ViewModels.Commands
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool Enabled
        {
            get => _canExecute;
            set
            {
                _canExecute = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private readonly Action _action;
        private bool _canExecute;

        public DelegateCommand(Action action, bool canExecute = true)
        {
            _action = action;
            Enabled = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return Enabled;
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }

    public class ParameterizedDelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action<object> _action;

        public ParameterizedDelegateCommand(Action<object> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
