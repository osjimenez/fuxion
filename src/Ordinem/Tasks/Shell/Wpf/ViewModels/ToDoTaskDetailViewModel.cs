using AutoMapper;
using Fuxion.Reflection;
using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Fuxion.Shell.ViewModels;
using Ordinem.Tasks.Domain;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Tasks.Shell.Wpf.ViewModels
{
	[TypeKey("Ordinem.Tasks.Shell.Wpf.ViewModels." + nameof(ToDoTaskDetailViewModel))]
	public class ToDoTaskDetailViewModel : DetailViewModel, IInitializablePanel
	{
		public ToDoTaskDetailViewModel(Cache cache, TasksProxy proxy, IMapper mapper)
		{
			this.cache = cache;
			this.proxy = proxy;
			this.mapper = mapper;
			
			var titleObs = this.WhenAnyValue(x => x.Dvo!.Name)
				.Select(x => $"Task '{x}'")
				.ObserveOn(RxApp.MainThreadScheduler);
			_Title = titleObs.ToProperty(this, x => x.Title, "Loading ...");
			_Header = titleObs.ToProperty(this, x => x.Header, "Loading ...");

			RenameCommand = ReactiveCommand.CreateFromTask(async () =>
			{
				if (Dvo != null)
				{
					var newName = await RenameInteraction.Handle(Dvo.Name);
					if (!string.IsNullOrWhiteSpace(newName))
					{
						await proxy.RenameToDoTask(Dvo.Id, newName);
						Dvo.Name = newName;
					}
				}
			});
		}
		~ToDoTaskDetailViewModel() => Debug.WriteLine($"||||||||||||||||||||||||||||||||| => {GetType().Name} DESTROYED");

		private readonly Cache cache;
		private readonly TasksProxy proxy;
		private readonly IMapper mapper;

		private readonly ObservableAsPropertyHelper<string> _Title;
		public string Title => _Title.Value;
		private readonly ObservableAsPropertyHelper<string> _Header;
		public string Header => _Header.Value;

		ToDoTaskDvo? _Dvo;
		public ToDoTaskDvo? Dvo {
			get => _Dvo;
			set => this.RaiseAndSetIfChanged(ref _Dvo, value);
		}

		public async void Initialize(PanelName name, Dictionary<string, object> args)
		{
			// TODO - Cambiar el string del argumento por una constante. Mejor aun, cambiar los argumentos por una clase especifica que se pueda extender an cada modulo: args.Id()
			// if args don't contains 'Id' is because panel was created from a saved layout
			var id = args.ContainsKey("Id") ? (Guid)args["Id"] : Guid.Parse(name.Key);
			var list = cache.Get<ToDoTaskDvo>();
			Dvo = list.Items.FirstOrDefault(_ => _.Id == id);
			if (Dvo == null)
			{
				cache.AddOrUpdate(mapper.Map<ToDoTaskDvo>(await proxy.GetToDoTask(id)));
				//cache.EditDiff(mapper.Map<ToDoTaskDvo>(await proxy.GetToDoTask(id)));
				Dvo = list.Items.FirstOrDefault(_ => _.Id == id);
			}
			MessageBus.Current.Listen<UpdatePanelMessage>().Subscribe(msg =>
			{
				if (msg.UpdatedDvo.Id == Dvo.Id && msg.UpdatedDvo is ToDoTaskDvo dvo)
					Dvo = dvo;
			});
		}
		public Interaction<string?, string> RenameInteraction { get; } = new Interaction<string?, string>();
		public ReactiveCommand<Unit, Unit> RenameCommand { get; }
	}
}
