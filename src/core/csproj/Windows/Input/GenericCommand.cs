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
		bool ICommand.CanExecute(object parameter) => CanExecute();
		public Task<bool> CanExecuteAsync() => this.Invoke(() => canExecute != null ? canExecute() : true);
		public bool CanExecute() => CanExecuteAsync().Result;
		public Task RaiseCanExecuteChangedAsync() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
		public void RaiseCanExecuteChanged() => RaiseCanExecuteChangedAsync().Wait();

		void ICommand.Execute(object parameter) => Execute();
		public Task ExecuteAsync() => this.Invoke(action);
		public void Execute() => ExecuteAsync().Wait();
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
				return CanExecute(par);
			if (parameter == null && typeof(TParameter).IsNullable())
				return CanExecute(default(TParameter));
			throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
		}
		public Task<bool> CanExecuteAsync(TParameter parameter) => this.Invoke(canExecute, parameter);
		public bool CanExecute(TParameter parameter) => CanExecuteAsync(parameter).Result;
		public Task RaiseCanExecuteChangedAsync() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
		public void RaiseCanExecuteChanged() => RaiseCanExecuteChangedAsync().Wait();

		void ICommand.Execute(object parameter)
		{
			if (parameter is TParameter par)
				Execute(par);
			else if (typeof(TParameter).IsNullable() && parameter == null)
				Execute(default(TParameter));
			else
				throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
		}
		public Task ExecuteAsync(TParameter parameter) => this.Invoke(action, parameter);
		public void Execute(TParameter parameter) => ExecuteAsync(parameter).Wait();
	}
}