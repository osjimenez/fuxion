using AutoMapper;
using Fuxion.Reflection;
using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Fuxion.Shell.ViewModels;
using Ordinem.Calendar.Domain;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordinem.Shell.Wpf.Calendar.ViewModels
{
	[TypeKey("Ordinem.Shell.Wpf.Calendar.ViewModels." + nameof(AppointmentDetailViewModel))]
	public class AppointmentDetailViewModel : DetailViewModel, IInitializablePanel
	{
		public AppointmentDetailViewModel(Cache cache, CalendarProxy proxy, IMapper mapper)
		{
			this.cache = cache;
			this.proxy = proxy;
			this.mapper = mapper;
			
			var titleObs = this.WhenAnyValue(x => x.Dvo.Name)
				.Select(x => $"Appointment '{x}'")
				.ObserveOn(RxApp.MainThreadScheduler);
			_Title = titleObs.ToProperty(this, x => x.Title, "Loading ...");
			_Header = titleObs.ToProperty(this, x => x.Header, "Loading ...");

			RenameCommand = ReactiveCommand.CreateFromTask(async () =>
			{
				var newName = await RenameInteraction.Handle(Dvo.Name);
				if (!string.IsNullOrWhiteSpace(newName))
				{
					await proxy.RenameToDoTask(Dvo.Id, newName);
					Dvo.Name = newName;
				}
			});
		}
		~AppointmentDetailViewModel() => Debug.WriteLine($"||||||||||||||||||||||||||||||||| => {GetType().Name} DESTROYED");

		private readonly Cache cache;
		private readonly CalendarProxy proxy;
		private readonly IMapper mapper;

		private readonly ObservableAsPropertyHelper<string> _Title;
		public string Title => _Title.Value;
		private readonly ObservableAsPropertyHelper<string> _Header;
		public string Header => _Header.Value;

		AppointmentDvo _Dvo;
		public AppointmentDvo Dvo {
			get => _Dvo;
			set => this.RaiseAndSetIfChanged(ref _Dvo, value);
		}

		public async void Initialize(PanelName name, Dictionary<string, object> args)
		{
			// TODO - Cambiar el string del argumento por una constante. Mejor aun, cambiar los argumentos por una clase especifica que se pueda extender an cada modulo: args.Id()
			// if args don't contains 'Id' is because panel was created from a saved layout
			var id = args.ContainsKey("Id") ? (Guid)args["Id"] : Guid.Parse(name.Key);
			var list = cache.Get<AppointmentDvo>();
			Dvo = list.Items.FirstOrDefault(_ => _.Id == id);
			if (Dvo == null)
			{
				cache.AddOrUpdate(mapper.Map<AppointmentDvo>(await proxy.GetAppointment(id)));
				//cache.EditDiff(mapper.Map<ToDoTaskDvo>(await proxy.GetToDoTask(id)));
				Dvo = list.Items.FirstOrDefault(_ => _.Id == id);
			}
			MessageBus.Current.Listen<UpdatePanelMessage>().Subscribe(msg =>
			{
				if (msg.UpdatedDvo.Id == Dvo.Id && msg.UpdatedDvo is AppointmentDvo dvo)
					Dvo = dvo;
			});
		}
		public Interaction<string, string> RenameInteraction { get; } = new Interaction<string, string>();
		public ReactiveCommand<Unit, Unit> RenameCommand { get; }
	}
}
