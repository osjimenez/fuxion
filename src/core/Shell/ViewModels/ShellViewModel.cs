using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.ViewModels
{
	public abstract class ShellViewModel : ReactiveObject, IActivatableViewModel, ICompositeDisposable
	{
		public ShellViewModel(ILogger<ShellViewModel> logger)
		{
			Activator = new ViewModelActivator();

			Logger = logger;
			Logger.LogTrace($"ShellViewModel '{GetType().Name}' CREATED");

			this.WhenActivated(d =>
			{
				HandleActivation();
				Disposable.Create(() => HandleDeactivation())
					.DisposeWith(d);
			});
		}
		public ViewModelActivator Activator { get; }
		protected ILogger Logger { get; }
#if DEBUG
		~ShellViewModel() => Logger.LogTrace($"ShellViewModel '{GetType().Name}' FINALIZED");
#endif
		protected virtual void HandleActivation() { }
		protected virtual void HandleDeactivation() { }

		CompositeDisposable ICompositeDisposable.CompositeDisposable { get; } = new();
		bool ICompositeDisposable.Disposed { get; set; }
	}
}
