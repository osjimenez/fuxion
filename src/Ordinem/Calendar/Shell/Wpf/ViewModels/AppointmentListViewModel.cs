using AutoMapper;
using DynamicData;
using Fuxion.Collections.Generic;
using Fuxion.Reflection;
using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Fuxion.Shell.ViewModels;
using Ordinem.Calendar.Domain;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Telerik.Windows.Data;

namespace Ordinem.Calendar.Shell.Wpf.ViewModels
{
	[TypeKey("Ordinem.Shell.Wpf.Calendar.ViewModels." + nameof(AppointmentListViewModel))]
	public class AppointmentListViewModel : ListViewModel<AppointmentDvo>, IPanel
	{
		public AppointmentListViewModel(Cache cache, CalendarProxy proxy, IMapper mapper)
		{
			CreateCommand = ReactiveCommand.CreateFromTask(async () =>
			{
				var id = Guid.NewGuid();
				var name = await CreateInteraction.Handle(Unit.Default);
				if (name != null)
				{
					await proxy.AddToDoTask(id, name);
					var dto = await proxy.GetAppointment(id);
					var dvo = mapper.Map<AppointmentDvo>(dto);
					cache.Get<AppointmentDvo>().AddOrUpdate(dvo);
					MessageBus.Current.OpenAppointment(dvo.Id);
				}
			});
			EditCommand = ReactiveCommand.Create<AppointmentDvo>(
				toDoTask => MessageBus.Current.OpenAppointment(toDoTask.Id),
				this.WhenAnyValue(x => x.SelectedToDoTask).Select(d => d != null));
			DeleteCommand = ReactiveCommand.Create<AppointmentDvo>(async toDoTask => 
			{
				await proxy.DeleteToDoTask(toDoTask.Id);
				cache.Get<AppointmentDvo>().Remove(toDoTask);
				MessageBus.Current.CloseAllPanelsWithKey(toDoTask.Id.ToString());
			}, this.WhenAnyValue(x => x.SelectedToDoTask).Select(d => d != null));
			RefreshCommand = ReactiveCommand.CreateFromTask(async cancellationToken =>
			{
				cache.Get<AppointmentDvo>().EditDiff(
					mapper.Map<IEnumerable<AppointmentDto>, IList<AppointmentDvo>>(await proxy.GetAppointments()),
					// TODO - Implementar un mecanismo para detectar los cambios basado en un timestamp o similar
					(t1, t2) => t1.Name == t2.Name);
			});
			cache.Get<AppointmentDvo>()
				.Connect()
				.OnItemRemoved(toDoTask => MessageBus.Current.CloseAllPanelsWithKey(toDoTask.Id.ToString()))
				.OnItemUpdated((updatedToDoTask, _) => MessageBus.Current.SendMessage(new UpdatePanelMessage(updatedToDoTask)))
				
				//.AutoRefresh(_ => _.Name)
				.AutoRefresh()

				//.Filter(_ => _.Name != null)

				.Sort(new GenericComparer<AppointmentDvo>((t1, t2) => StringComparer.CurrentCulture.Compare(t1.Name, t2.Name)))
				.Page(this.WhenAnyValue(_ => _.PageRequest))
				
				.ObserveOnDispatcher()
				.Bind(out var list)
				.DisposeMany()
				.Subscribe();
			ToDoTasks = list;

			cache.Get<AppointmentDvo>()
				.Connect()
				.ObserveOnDispatcher()
				.Bind(out var allList)
				.DisposeMany()
				.Subscribe();
			AllToDoTasks = allList;

			// TODO - Cancel in progress command https://github.com/reactiveui/ReactiveUI/issues/1536
			RefreshCommand.Execute().Subscribe();
			//RefreshCommand.Execute().Subscribe();
		}
		~AppointmentListViewModel() => Debug.WriteLine($"||||||||||||||||||||||||||||||||| => {this.GetType().Name} DESTROYED");

		public string Title => "Eventos";
		public string Header => "Eventos";
		
		public ReadOnlyObservableCollection<AppointmentDvo> AllToDoTasks { get; }
		public ReadOnlyObservableCollection<AppointmentDvo> ToDoTasks { get; }
		AppointmentDvo _SelectedToDoTask;
		public AppointmentDvo SelectedToDoTask
		{
			get => _SelectedToDoTask;
			set => this.RaiseAndSetIfChanged(ref _SelectedToDoTask, value);
		}
		PageRequest _PageRequest = new PageRequest(1, 5);
		public PageRequest PageRequest
		{
			get => _PageRequest;
			set => this.RaiseAndSetIfChanged(ref _PageRequest, value);
		}

		public Interaction<Unit, string> CreateInteraction { get; } = new Interaction<Unit, string>();
		public ReactiveCommand<Unit, Unit> CreateCommand { get; }
		public ReactiveCommand<AppointmentDvo, Unit> EditCommand { get; }
		public ReactiveCommand<AppointmentDvo, Unit> DeleteCommand { get; }
		public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
	}
}
