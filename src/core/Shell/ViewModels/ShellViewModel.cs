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
	public class ShellViewModel : BaseViewModel
	{
		public ShellViewModel(ILogger<ShellViewModel> logger) : base(logger) { }
		//public ShellViewModel()
		//{
		//	MessageBus.Current.Listen<OpenPanelUIMessage>()
		//		.Subscribe(message => ConsoleText += "\r\n" + message.Name);

		//	_ReadOnlyConsoleText = MessageBus.Current.Listen<OpenPanelUIMessage>()
		//		.Select(message => ReadOnlyConsoleText + "\r\n" + message.Name)
		//		//.Select(message => message.PanelName)
		//		.ObserveOn(RxApp.MainThreadScheduler)
		//		.ToProperty(this, x => x.ReadOnlyConsoleText);

		//	ConsoleText = "Oscar";
		//}
		//private string _ConsoleText = "KK";
		//public string ConsoleText
		//{
		//	get => _ConsoleText;
		//	set => this.RaiseAndSetIfChanged(ref _ConsoleText, value);
		//}
		//private readonly ObservableAsPropertyHelper<string> _ReadOnlyConsoleText;
		//public string ReadOnlyConsoleText => _ReadOnlyConsoleText.Value;
		//public ReactiveCommand<Unit, Unit> SendCommand { get; }
	}
}
