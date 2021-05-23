using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Shell.ViewModels
{
	public abstract class BaseViewModel : ReactiveObject
	{
		public BaseViewModel(ILogger<BaseViewModel> logger)
		{
			Logger = logger;
			Logger.LogTrace($"||||||||||||||||||||||||||||||||| => {GetType().Name} CREATED");
		}
		protected ILogger Logger { get; }
		~BaseViewModel() => Logger.LogTrace($"||||||||||||||||||||||||||||||||| => {GetType().Name} DESTROYED");
	}
}
