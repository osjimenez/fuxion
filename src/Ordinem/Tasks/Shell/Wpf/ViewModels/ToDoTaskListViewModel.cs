using AutoMapper;
using DynamicData;
using Fuxion.Collections.Generic;
using Fuxion.Reflection;
using Fuxion.Shell;
using Fuxion.Shell.Messages;
using Fuxion.Shell.ViewModels;
using Ordinem.Tasks.Domain;
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

namespace Ordinem.Tasks.Shell.Wpf.ViewModels
{
	[TypeKey("Ordinem.Tasks.Shell.Wpf.ViewModels." + nameof(ToDoTaskListViewModel))]
	public class ToDoTaskListViewModel : ListViewModel<ToDoTaskDvo>, IPanel
	{
		public ToDoTaskListViewModel(Cache cache, TasksProxy proxy, IMapper mapper)
		{
			CreateToDoTaskCommand = ReactiveCommand.CreateFromTask(async () =>
			{
				var id = Guid.NewGuid();
				var name = await CreateInteraction.Handle(Unit.Default);
				if (name != null)
				{
					await proxy.AddToDoTask(id, name);
					var dto = await proxy.GetToDoTask(id);
					var dvo = mapper.Map<ToDoTaskDvo>(dto);
					cache.Get<ToDoTaskDvo>().AddOrUpdate(dvo);
					MessageBus.Current.OpenToDoTask(dvo.Id);
				}
			});
			EditToDoTaskCommand = ReactiveCommand.Create<ToDoTaskDvo>(
				toDoTask => MessageBus.Current.OpenToDoTask(toDoTask.Id),
				this.WhenAnyValue(x => x.SelectedToDoTask).Select(d => d != null));
			DeleteToDoTaskCommand = ReactiveCommand.Create<ToDoTaskDvo>(async toDoTask => 
			{
				await proxy.DeleteToDoTask(toDoTask.Id);
				cache.Get<ToDoTaskDvo>().Remove(toDoTask);
				MessageBus.Current.CloseAllPanelsWithKey(toDoTask.Id.ToString());
			}, this.WhenAnyValue(x => x.SelectedToDoTask).Select(d => d != null));
			RefreshCommand = ReactiveCommand.CreateFromTask(async cancellationToken =>
			{
				cache.Get<ToDoTaskDvo>().EditDiff(
					mapper.Map<IEnumerable<ToDoTaskDto>, IList<ToDoTaskDvo>>(await proxy.GetToDoTasks()),
					// TODO - Implementar un mecanismo para detectar los cambios basado en un timestamp o similar
					(t1, t2) => t1.Name == t2.Name);
			});
			cache.Get<ToDoTaskDvo>()
				.Connect()
				.OnItemRemoved(toDoTask => MessageBus.Current.CloseAllPanelsWithKey(toDoTask.Id.ToString()))
				.OnItemUpdated((updatedToDoTask, _) => MessageBus.Current.SendMessage(new UpdatePanelMessage(updatedToDoTask)))
				
				//.AutoRefresh(_ => _.Name)
				.AutoRefresh()

				//.Filter(_ => _.Name != null)

				.Sort(new GenericComparer<ToDoTaskDvo>((t1, t2) => StringComparer.CurrentCulture.Compare(t1.Name, t2.Name)))
				.Page(this.WhenAnyValue(_ => _.PageRequest))
				
				.ObserveOnDispatcher()
				.Bind(out var list)
				.DisposeMany()
				.Subscribe();
			ToDoTasks = list;

			cache.Get<ToDoTaskDvo>()
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
		~ToDoTaskListViewModel() => Debug.WriteLine($"||||||||||||||||||||||||||||||||| => {this.GetType().Name} DESTROYED");

		public string Title => "Tareas";
		public string Header => "Tareas";
		
		public ReadOnlyObservableCollection<ToDoTaskDvo> AllToDoTasks { get; }
		public ReadOnlyObservableCollection<ToDoTaskDvo> ToDoTasks { get; }
		ToDoTaskDvo _SelectedToDoTask;
		public ToDoTaskDvo SelectedToDoTask
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
		public ReactiveCommand<Unit, Unit> CreateToDoTaskCommand { get; }
		public ReactiveCommand<ToDoTaskDvo, Unit> EditToDoTaskCommand { get; }
		public ReactiveCommand<ToDoTaskDvo, Unit> DeleteToDoTaskCommand { get; }
		public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
	}
}
