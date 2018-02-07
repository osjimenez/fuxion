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
		bool ICommand.CanExecute(object parameter) => CanExecute();
		public bool CanExecute() => canExecute != null ? canExecute() : true;
		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

		void ICommand.Execute(object parameter) => Execute();
		public void Execute() => action();
		
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
		bool ICommand.CanExecute(object parameter)
		{
			if (parameter is TParameter par)
				return canExecute != null ? CanExecute(par) : true;
			if (typeof(TParameter).IsNullable() && parameter == null)
				return canExecute != null ? CanExecute(default(TParameter)) : true;
			throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
		}
		public bool CanExecute(TParameter parameter) => canExecute(parameter);
		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

		void ICommand.Execute(object parameter)
		{
			if (parameter is TParameter par)
				Execute(par);
			else if (typeof(TParameter).IsNullable() && parameter == null)
				Execute(default(TParameter));
			else
				throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
		}
		public void Execute(TParameter parameter) => action(parameter);
		
	}
}
