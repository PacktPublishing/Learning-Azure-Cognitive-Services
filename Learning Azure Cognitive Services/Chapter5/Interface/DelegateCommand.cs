using System;
using System.Windows.Input;

namespace Chapter5.Interface
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        /// <summary>
        /// Constructor for DelegateCommand base class
        /// Sets the execute action and canExecute predicate
        /// </summary>
        /// <param name="execute">An action, specifying what to do if this command is triggered</param>
        /// <param name="canExecute">Predicate to indicate if we can execute this command. Can be null</param>
        public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns true if we can execute the command, false otherwise
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;

            return _canExecute(parameter);
        }

        /// <summary>
        /// Executes the action corresponding to the command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// EventHandler notifying listeners about changes to CanExecute
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}