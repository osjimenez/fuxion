using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Fuxion.Windows.Threading;

namespace Fuxion.Windows.Input
{
	public class GenericCommand : ICommand, IDispatchable
	{
		public GenericCommand(Action action, Func<bool> canExecute = null)
		{
			this.action = action;
			this.canExecute = canExecute;
		}

		Action action;
		Func<bool> canExecute;

		bool IDispatchable.UseDispatcher { get; set; } = true;

		public event EventHandler CanExecuteChanged;
		bool ICommand.CanExecute(object parameter) => CanExecute().Result;
		public Task<bool> CanExecute() => this.Invoke(() => canExecute != null ? canExecute() : true);
		public Task RaiseCanExecuteChanged() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));

		void ICommand.Execute(object parameter) => Execute().Wait();
		public Task Execute() => this.Invoke(action);
	}
	public class GenericCommand<TParameter> : ICommand, IDispatchable
	{
		public GenericCommand(Action<TParameter> action, Func<TParameter, bool> canExecute = null)
		{
			this.action = action;
			this.canExecute = canExecute;
		}

		Action<TParameter> action;
		Func<TParameter, bool> canExecute;

		bool IDispatchable.UseDispatcher { get; set; } = true;

		public event EventHandler CanExecuteChanged;
		bool ICommand.CanExecute(object parameter)
		{
			if (parameter is TParameter par)
				return CanExecute(par).Result;
			if (parameter == null && typeof(TParameter).IsNullable())
				return CanExecute(default(TParameter)).Result;
			throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
		}
		public Task<bool> CanExecute(TParameter parameter) => this.Invoke(canExecute, parameter);
		public Task RaiseCanExecuteChanged() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));

		void ICommand.Execute(object parameter)
		{
			if (parameter is TParameter par)
				Execute(par).Wait();
			else if (typeof(TParameter).IsNullable() && parameter == null)
				Execute(default(TParameter)).Wait();
			else
				throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
		}
		public Task Execute(TParameter parameter) => this.Invoke(action, parameter);
	}
}