using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fuxion.Logging;
namespace Fuxion.Threading.Tasks
{
	public static class TaskManager
	{
		readonly internal static Locker<List<ITaskManagerEntry>> Tasks = new Locker<List<ITaskManagerEntry>>(new List<ITaskManagerEntry>());
		private static void AddEntry(ITaskManagerEntry entry)
		{
			entry.Task.ContinueWith((t) => Tasks.Write(l =>
			{
				l.Remove(entry);
			}));
			Tasks.Write(l =>
			{
				l.Add(entry);
			});
		}
		internal static ITaskManagerEntry SearchEntry(Task task, bool throwExceptionIfNotFound = true)
		{
			//Busco entre las tareas administradas
			var res = Tasks.Read(l => l.FirstOrDefault(e => e.Task == task));
			//Compruebo si se encontró y si debo lanzar una excepción
			if (res == null && throwExceptionIfNotFound)
				throw new ArgumentException("Task wasn't created with TaskManager.");
			return res;
		}
		public static Task Current => Tasks.Read(l => l.FirstOrDefault(e => Task.CurrentId.HasValue && e.Task.Id == Task.CurrentId.Value)).Task;

		#region Void
		private static ITaskManagerEntry CreateEntry(Action action, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(action, scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create(Action action, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, null, options, concurrencyProfile).Task;
		public static Task Create(Func<Task> func, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action(() => func().Wait()), null, options, concurrencyProfile, func).Task;
		public static Task StartNew(Action action, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew(Func<Task> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action(() => func().Wait()), scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T>
		private static ITaskManagerEntry CreateEntry<T>(Action<T> action, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o => action((T)o)), param, scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T>(Action<T> action, T param, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param, null, options, concurrencyProfile).Task;
		public static Task Create<T>(Func<T, Task> func, T param, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T>(p => func(p).Wait()), param, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T>(Action<T> action, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T>(Func<T, Task> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T>(p => func(p).Wait()), param, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2>
		private static ITaskManagerEntry CreateEntry<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2) = (ValueTuple<T1, T2>)o;
				action.Invoke(p1, p2);
			}), (param1, param2), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2>(Func<T1, T2, Task> func, T1 param1, T2 param2, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2>((p1, p2) => func(p1, p2).Wait()), param1, param2, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2>(Func<T1, T2, Task> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2>((p1, p2) => func(p1, p2).Wait()), param1, param2, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3) = (ValueTuple<T1, T2, T3>)o;
				action.Invoke(p1, p2, p3);
			}), (param1, param2, param3), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3>(Func<T1, T2, T3, Task> func, T1 param1, T2 param2, T3 param3, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3>((p1, p2, p3) => func(p1, p2, p3).Wait()), param1, param2, param3, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3>(Func<T1, T2, T3, Task> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3>((p1, p2, p3) => func(p1, p2, p3).Wait()), param1, param2, param3, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3, T4>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3, p4) = (ValueTuple<T1, T2, T3, T4>)o;
				action.Invoke(p1, p2, p3, p4);
			}), (param1, param2, param3, param4), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, param4, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3, T4>((p1, p2, p3, p4) => func(p1, p2, p3, p4).Wait()), param1, param2, param3, param4, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, param4, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3, T4>((p1, p2, p3, p4) => func(p1, p2, p3, p4).Wait()), param1, param2, param3, param4, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3, T4, T5>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3, p4, p5) = (ValueTuple<T1, T2, T3, T4, T5>)o;
				action.Invoke(p1, p2, p3, p4, p5);
			}), (param1, param2, param3, param4, param5), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, param4, param5, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3, T4, T5>((p1, p2, p3, p4, p5) => func(p1, p2, p3, p4, p5).Wait()), param1, param2, param3, param4, param5, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, param4, param5, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3, T4, T5>((p1, p2, p3, p4, p5) => func(p1, p2, p3, p4, p5).Wait()), param1, param2, param3, param4, param5, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3, T4, T5, T6>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3, p4, p5, p6) = (ValueTuple<T1, T2, T3, T4, T5, T6>)o;
				action.Invoke(p1, p2, p3, p4, p5, p6);
			}), (param1, param2, param3, param4, param5, param6), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, param4, param5, param6, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3, T4, T5, T6>((p1, p2, p3, p4, p5, p6) => func(p1, p2, p3, p4, p5, p6).Wait()), param1, param2, param3, param4, param5, param6, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, param4, param5, param6, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3, T4, T5, T6>((p1, p2, p3, p4, p5, p6) => func(p1, p2, p3, p4, p5, p6).Wait()), param1, param2, param3, param4, param5, param6, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3, T4, T5, T6, T7>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3, p4, p5, p6, p7) = (ValueTuple<T1, T2, T3, T4, T5, T6, T7>)o;
				action.Invoke(p1, p2, p3, p4, p5, p6, p7);
			}), (param1, param2, param3, param4, param5, param6, param7), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, param4, param5, param6, param7, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7>((p1, p2, p3, p4, p5, p6, p7) => func(p1, p2, p3, p4, p5, p6, p7).Wait()), param1, param2, param3, param4, param5, param6, param7, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, param4, param5, param6, param7, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7>((p1, p2, p3, p4, p5, p6, p7) => func(p1, p2, p3, p4, p5, p6, p7).Wait()), param1, param2, param3, param4, param5, param6, param7, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3, T4, T5, T6, T7, T8>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3, p4, p5, p6, p7, p8) = (ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>>)o;
				action.Invoke(p1, p2, p3, p4, p5, p6, p7, p8);
			}), (param1, param2, param3, param4, param5, param6, param7, param8), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, param4, param5, param6, param7, param8, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7, T8>((p1, p2, p3, p4, p5, p6, p7, p8) => func(p1, p2, p3, p4, p5, p6, p7, p8).Wait()), param1, param2, param3, param4, param5, param6, param7, param8, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7, T8>((p1, p2, p3, p4, p5, p6, p7, p8) => func(p1, p2, p3, p4, p5, p6, p7, p8).Wait()), param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Void<T1,T2, T3, T4, T5, T6, T7, T8, T9>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new ActionTaskManagerEntry(new Action<object>(o =>
			{
				var (p1, p2, p3, p4, p5, p6, p7, p8, p9) = (ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9>>)o;
				action.Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9);
			}), (param1, param2, param3, param4, param5, param6, param7, param8, param9), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(action, param1, param2, param3, param4, param5, param6, param7, param8, param9, null, options, concurrencyProfile).Task;
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>((p1, p2, p3, p4, p5, p6, p7, p8, p9) => func(p1, p2, p3, p4, p5, p6, p7, p8, p9).Wait()), param1, param2, param3, param4, param5, param6, param7, param8, param9, null, options, concurrencyProfile, func).Task;
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(action, param1, param2, param3, param4, param5, param6, param7, param8, param9, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>((p1, p2, p3, p4, p5, p6, p7, p8, p9) => func(p1, p2, p3, p4, p5, p6, p7, p8, p9).Wait()), param1, param2, param3, param4, param5, param6, param7, param8, param9, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<TResult>
		private static ITaskManagerEntry CreateEntry<TResult>(Func<TResult> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(func, scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task<TResult> Create<TResult>(Func<TResult> func, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<TResult>(Func<Task<TResult>> func, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<TResult>(() => func().Result), null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<TResult>(Func<TResult> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, scheduler, options, concurrencyProfile).Task; ;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<TResult>(Func<Task<TResult>> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<TResult>(() => func().Result), scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T, TResult>
		private static ITaskManagerEntry CreateEntry<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(
				new Func<object, TResult>(o => func((T)o)),
				param, scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task<TResult> Create<T, TResult>(Func<T, TResult> func, T param, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T, TResult>(Func<T, Task<TResult>> func, T param, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T, TResult>(p => func(p).Result), param, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T, TResult>(Func<T, Task<TResult>> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T, TResult>(p => func(p).Result), param, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2) = (ValueTuple<T1, T2>)o;
				return func(p1, p2);
			}), (param1, param2), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func, T1 param1, T2 param2, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, TResult>((p1, p2) => func(p1, p2).Result), param1, param2, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, Task<TResult>> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, TResult>((p1, p2) => func(p1, p2).Result), param1, param2, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3) = (ValueTuple<T1, T2, T3>)o;
				return func(p1, p2, p3);
			}), (param1, param2, param3), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> func, T1 param1, T2 param2, T3 param3, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, TResult>((p1, p2, p3) => func(p1, p2, p3).Result), param1, param2, param3, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, TResult>((p1, p2, p3) => func(p1, p2, p3).Result), param1, param2, param3, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, T4, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3, p4) = (ValueTuple<T1, T2, T3, T4>)o;
				return func(p1, p2, p3, p4);
			}), (param1, param2, param3, param4), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, TResult>((p1, p2, p3, p4) => func(p1, p2, p3, p4).Result), param1, param2, param3, param4, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, TResult>((p1, p2, p3, p4) => func(p1, p2, p3, p4).Result), param1, param2, param3, param4, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, T4, T5, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3, p4, p5) = (ValueTuple<T1, T2, T3, T4, T5>)o;
				return func(p1, p2, p3, p4, p5);
			}), (param1, param2, param3, param4, param5), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;
		}
		public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, TResult>((p1, p2, p3, p4, p5) => func(p1, p2, p3, p4, p5).Result), param1, param2, param3, param4, param5, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, TResult>((p1, p2, p3, p4, p5) => func(p1, p2, p3, p4, p5).Result), param1, param2, param3, param4, param5, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, T4, T5, T6, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3, p4, p5, p6) = (ValueTuple<T1, T2, T3, T4, T5, T6>)o;
				return func(p1, p2, p3, p4, p5, p6);
			}), (param1, param2, param3, param4, param5, param6), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;

		}
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, TResult>((p1, p2, p3, p4, p5, p6) => func(p1, p2, p3, p4, p5, p6).Result), param1, param2, param3, param4, param5, param6, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, TResult>((p1, p2, p3, p4, p5, p6) => func(p1, p2, p3, p4, p5, p6).Result), param1, param2, param3, param4, param5, param6, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, T4, T5, T6, T7, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3, p4, p5, p6, p7) = (ValueTuple<T1, T2, T3, T4, T5, T6, T7>)o;
				return func(p1, p2, p3, p4, p5, p6, p7);
			}), (param1, param2, param3, param4, param5, param6, param7), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;

		}
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, TResult>((p1, p2, p3, p4, p5, p6, p7) => func(p1, p2, p3, p4, p5, p6, p7).Result), param1, param2, param3, param4, param5, param6, param7, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, TResult>((p1, p2, p3, p4, p5, p6, p7) => func(p1, p2, p3, p4, p5, p6, p7).Result), param1, param2, param3, param4, param5, param6, param7, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3, p4, p5, p6, p7, p8) = (ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>>)o;
				return func(p1, p2, p3, p4, p5, p6, p7, p8);
			}), (param1, param2, param3, param4, param5, param6, param7, param8), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;

		}
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, param8, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>((p1, p2, p3, p4, p5, p6, p7, p8) => func(p1, p2, p3, p4, p5, p6, p7, p8).Result), param1, param2, param3, param4, param5, param6, param7, param8, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>((p1, p2, p3, p4, p5, p6, p7, p8) => func(p1, p2, p3, p4, p5, p6, p7, p8).Result), param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
		#region Result<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile), Delegate @delegate = null)
		{
			var entry = new FuncTaskManagerEntry<TResult>(new Func<object, TResult>(o =>
			{
				var (p1, p2, p3, p4, p5, p6, p7, p8, p9) = (ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9>>)o;
				return func(p1, p2, p3, p4, p5, p6, p7, p8, p9);
			}), (param1, param2, param3, param4, param5, param6, param7, param8, param9), scheduler, options, concurrencyProfile, @delegate);
			AddEntry(entry);
			return entry;

		}
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, param8, param9, null, options, concurrencyProfile).Task;
		public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
			=> (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>((p1, p2, p3, p4, p5, p6, p7, p8, p9) => func(p1, p2, p3, p4, p5, p6, p7, p8, p9).Result), param1, param2, param3, param4, param5, param6, param7, param8, param9, null, options, concurrencyProfile, func).Task;
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, param8, param9, scheduler, options, concurrencyProfile).Task;
			SearchEntry(task).Start();
			return task;
		}
		public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<TResult>> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions), ConcurrencyProfile concurrencyProfile = default(ConcurrencyProfile))
		{
			var task = (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>((p1, p2, p3, p4, p5, p6, p7, p8, p9) => func(p1, p2, p3, p4, p5, p6, p7, p8, p9).Result), param1, param2, param3, param4, param5, param6, param7, param8, param9, scheduler, options, concurrencyProfile, func).Task;
			SearchEntry(task).Start();
			return task;
		}
		#endregion
	}
}
