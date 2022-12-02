using System.Windows.Input;
using Fuxion.Windows.Threading;

namespace Fuxion.Windows.Input;

public class GenericCommand : ICommand, IInvokable
{
	public GenericCommand(Action action, Func<bool>? canExecute = null)
	{
		this.action = action;
		this.canExecute = canExecute;
	}
	readonly Action action;
	readonly Func<bool>? canExecute;
	public event EventHandler? CanExecuteChanged;
	bool ICommand.CanExecute(object? parameter) => CanExecute();
	void ICommand.Execute(object? parameter) => Execute();
	bool IInvokable.UseInvoker { get; set; } = true;
	public Task<bool> CanExecuteAsync() => canExecute != null ? this.Invoke(canExecute) : Task.FromResult(true);
	public bool CanExecute() => CanExecuteAsync().Result;
	public Task RaiseCanExecuteChangedAsync() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
	public void RaiseCanExecuteChanged() => RaiseCanExecuteChangedAsync().Wait();
	public Task ExecuteAsync() => this.Invoke(action);
	public void Execute() => ExecuteAsync().Wait();
}

public class GenericCommand<TParameter> : ICommand, IInvokable
{
	public GenericCommand(Action<TParameter> action, Func<TParameter, bool>? canExecute = null)
	{
		this.action = action;
		this.canExecute = canExecute;
	}
	readonly Action<TParameter> action;
	readonly Func<TParameter, bool>? canExecute;
	public event EventHandler? CanExecuteChanged;
	bool ICommand.CanExecute(object? parameter)
	{
		if (parameter is null) throw new InvalidOperationException($"The parameter '{nameof(parameter)}' cannot be null");
		if (parameter is TParameter par) return CanExecute(par);
		if (typeof(TParameter).IsNullable()) return CanExecute(default!);
		throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
	}
	void ICommand.Execute(object? parameter)
	{
		if (parameter is null) throw new InvalidOperationException($"The parameter '{nameof(parameter)}' cannot be null");
		if (parameter is TParameter par)
			Execute(par);
		else if (typeof(TParameter).IsNullable() && parameter == null)
			Execute(default!);
		else
			throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
	}
	bool IInvokable.UseInvoker { get; set; } = true;
	public Task<bool> CanExecuteAsync(TParameter parameter) => canExecute != null ? this.Invoke(canExecute, parameter) : Task.FromResult(true);
	public bool CanExecute(TParameter parameter) => CanExecuteAsync(parameter).Result;
	public Task RaiseCanExecuteChangedAsync() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
	public void RaiseCanExecuteChanged() => RaiseCanExecuteChangedAsync().Wait();
	public Task ExecuteAsync(TParameter parameter) => this.Invoke(action, parameter);
	public void Execute(TParameter parameter) => ExecuteAsync(parameter).Wait();
}