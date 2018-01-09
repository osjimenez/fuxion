using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Fuxion.Windows.Input
{
    public class GenericCommand : ICommand
    {
        public GenericCommand(Action action, Func<bool> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }
        Action action;
        Func<bool> canExecute;
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return canExecute != null ? canExecute() : true;
        }
        public void Execute(object parameter)
        {
            action();
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public class GenericCommand<TParameter> : ICommand
    {
        public GenericCommand(Action<TParameter> action, Func<TParameter, bool> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }
        Action<TParameter> action;
        Func<TParameter, bool> canExecute;
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            if (parameter is TParameter par)
                return canExecute != null ? canExecute(par) : true;
            if (typeof(TParameter).IsNullable() && parameter == null)
                return canExecute != null ? canExecute(default(TParameter)) : true;
            throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
        }
        public void Execute(object parameter)
        {
            if (parameter is TParameter par)
                action(par);
            else
                throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
