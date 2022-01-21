namespace Fuxion.Windows.Input;

using Fuxion.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

public class GenericCommand : ICommand, IInvokable
{
	public GenericCommand(Action action, Func<bool>? canExecute = null)
	{
		this.action = action;
		this.canExecute = canExecute;
	}

	private readonly Action action;
	private readonly Func<bool>? canExecute;

	bool IInvokable.UseInvoker { get; set; } = true;

	public event EventHandler? CanExecuteChanged;
	bool ICommand.CanExecute(object? parameter) => CanExecute();
	public Task<bool> CanExecuteAsync() => canExecute != null ? this.Invoke(canExecute) : Task.FromResult(true);
	public bool CanExecute() => CanExecuteAsync().Result;
	public Task RaiseCanExecuteChangedAsync() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
	public void RaiseCanExecuteChanged() => RaiseCanExecuteChangedAsync().Wait();

	void ICommand.Execute(object? parameter) => Execute();
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

	private readonly Action<TParameter> action;
	private readonly Func<TParameter, bool>? canExecute;

	bool IInvokable.UseInvoker { get; set; } = true;

	public event EventHandler? CanExecuteChanged;
	bool ICommand.CanExecute(object? parameter)
	{
		if (parameter is null) throw new InvalidOperationException($"The parameter '{nameof(parameter)}' cannot be null");
		if (parameter is TParameter par)
			return CanExecute(par);
		if (typeof(TParameter).IsNullable())
			return CanExecute(default!);
		throw new InvalidCastException($"The parameter of type '{parameter.GetType().Name}' couldn't casted to '{typeof(TParameter).Name}' as was declared for command parameter.");
	}
	public Task<bool> CanExecuteAsync(TParameter parameter) => canExecute != null ? this.Invoke(canExecute, parameter) : Task.FromResult(true);
	public bool CanExecute(TParameter parameter) => CanExecuteAsync(parameter).Result;
	public Task RaiseCanExecuteChangedAsync() => this.Invoke(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
	public void RaiseCanExecuteChanged() => RaiseCanExecuteChangedAsync().Wait();

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
	public Task ExecuteAsync(TParameter parameter) => this.Invoke(action, parameter);
	public void Execute(TParameter parameter) => ExecuteAsync(parameter).Wait();
}