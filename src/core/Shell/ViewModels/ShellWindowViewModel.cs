using Fuxion.Shell.Messages;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.ViewModels
{
	public class ShellWindowViewModel : ShellViewModel
	{
		public ShellWindowViewModel(ILogger<ShellWindowViewModel> logger) : base(logger) { }
	}
}
