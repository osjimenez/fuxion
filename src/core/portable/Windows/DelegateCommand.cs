using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
namespace Fuxion.Windows
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action action, Func<bool> canExecute = null)
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
}
