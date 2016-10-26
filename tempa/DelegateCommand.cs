using System;
using System.Windows.Input;

namespace CoffeeJelly.tempa
{
 
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// use this command when want to pass parameter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action<T> _action;

        public ActionCommand(Action<T> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                var castParameter = (T)Convert.ChangeType(parameter, typeof(T));
                _action(castParameter);
            }
        }
    }
}
