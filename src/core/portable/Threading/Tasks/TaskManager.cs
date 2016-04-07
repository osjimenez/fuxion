using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fuxion.Logging;
namespace Fuxion.Threading.Tasks
{
	/// <summary>
	/// Clase estática para la administración de tareas concurrentes
	/// </summary>
	public static class TaskManager
	{
		readonly static RefLocker<List<ITaskManagerEntry>> tasks = new RefLocker<List<ITaskManagerEntry>>(new List<ITaskManagerEntry>());
		private static void AddEntry(ITaskManagerEntry entry)
		{
			entry.Task.ContinueWith((t) => tasks.Write(l =>
			{
			    l.Remove(entry);
			}));
			tasks.Write(l => l.Add(entry));
            //tasks.Write(l => l.Add(entry));
		}
        internal static ITaskManagerEntry SearchEntry(Task task, bool throwExceptionIfNotFound = true)
        {
            //Busco entre las tareas administradas cual tiene el id
            var res = tasks.Read(l => l.FirstOrDefault(e => e.Task == task));
            //Compruebo si se encontró y debo lanzar una excepción
            if (res == null && throwExceptionIfNotFound)
                throw new ArgumentException("La tarea no esta administrada por el TaskManager." + task.CreationOptions.ToString()); // TODO ALMU: QUITAR "+task.CreationOptions.ToString()"
            return res;
        }

        #region Action
        private static ITaskManagerEntry CreateEntry(Action action, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var entry = new ActionTaskManagerEntry(action, scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create(Action action, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry(action, scheduler, options).Task;
		}
		public static Task Create(Action action) { return Create(action, null, default(TaskCreationOptions)); }
		public static Task Create(Action action, TaskScheduler scheduler) { return Create(action, scheduler, default(TaskCreationOptions)); }
		public static Task Create(Action action, TaskCreationOptions options) { return Create(action, null, options); }
		public static Task StartNew(Action action, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(action, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew(Action action) { return StartNew(action, null, default(TaskCreationOptions)); }
		public static Task StartNew(Action action, TaskScheduler scheduler) { return StartNew(action, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew(Action action, TaskCreationOptions options) { return StartNew(action, null, options); }
		#endregion
		#region Action<T>
		private static ITaskManagerEntry CreateEntry<T>(Action<T> action, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) => action.Invoke((T)obj));
			var entry = new ActionTaskManagerEntry(actionObj, param, scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T>(Action<T> action, T param, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T>(action, param, scheduler, options).Task;
		}
		public static Task Create<T>(Action<T> action, T param) { return Create<T>(action, param, null, default(TaskCreationOptions)); }
		public static Task Create<T>(Action<T> action, T param, TaskScheduler scheduler) { return Create<T>(action, param, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T>(Action<T> action, T param, TaskCreationOptions options) { return Create<T>(action, param, null, options); }
		public static Task StartNew<T>(Action<T> action, T param, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T>(action, param, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T>(Action<T> action, T param) { return StartNew<T>(action, param, null, default(TaskCreationOptions)); }
		public static Task StartNew<T>(Action<T> action, T param, TaskScheduler scheduler) { return StartNew<T>(action, param, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T>(Action<T> action, T param, TaskCreationOptions options) { return StartNew<T>(action, param, null, options); }
		#endregion
		#region Action<T1,T2>
		private static ITaskManagerEntry CreateEntry<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2>;
				action.Invoke(tuple.Item1, tuple.Item2);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2>(param1, param2), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2>(action, param1, param2, scheduler, options).Task;
		}
		public static Task Create<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) { return Create<T1, T2>(action, param1, param2, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler) { return Create<T1, T2>(action, param1, param2, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskCreationOptions options) { return Create<T1, T2>(action, param1, param2, null, options); }
		public static Task StartNew<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2>(action, param1, param2, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) { return StartNew<T1, T2>(action, param1, param2, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler) { return StartNew<T1, T2>(action, param1, param2, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskCreationOptions options) { return StartNew<T1, T2>(action, param1, param2, null, options); }
		#endregion
		#region Action<T1,T2, T3>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2, T3>;
				action.Invoke(tuple.Item1, tuple.Item2, tuple.Item3);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2, T3>(param1, param2, param3), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2, T3>(action, param1, param2, param3, scheduler, options).Task;
		}
		public static Task Create<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) { return Create<T1, T2, T3>(action, param1, param2, param3, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler) { return Create<T1, T2, T3>(action, param1, param2, param3, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskCreationOptions options) { return Create<T1, T2, T3>(action, param1, param2, param3, null, options); }
		public static Task StartNew<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2, T3>(action, param1, param2, param3, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) { return StartNew<T1, T2, T3>(action, param1, param2, param3, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler) { return StartNew<T1, T2, T3>(action, param1, param2, param3, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskCreationOptions options) { return StartNew<T1, T2, T3>(action, param1, param2, param3, null, options); }
		#endregion
		#region Action<T1,T2, T3, T4>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2, T3, T4>;
				action.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2, T3, T4>(param1, param2, param3, param4), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2, T3, T4>(action, param1, param2, param3, param4, scheduler, options).Task;
		}
		public static Task Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) { return Create<T1, T2, T3, T4>(action, param1, param2, param3, param4, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler) { return Create<T1, T2, T3, T4>(action, param1, param2, param3, param4, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options) { return Create<T1, T2, T3, T4>(action, param1, param2, param3, param4, null, options); }
		public static Task StartNew<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2, T3, T4>(action, param1, param2, param3, param4, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) { return StartNew<T1, T2, T3, T4>(action, param1, param2, param3, param4, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler) { return StartNew<T1, T2, T3, T4>(action, param1, param2, param3, param4, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options) { return StartNew<T1, T2, T3, T4>(action, param1, param2, param3, param4, null, options); }
		#endregion
		#region Action<T1,T2, T3, T4, T5>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2, T3, T4, T5>;
				action.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2, T3, T4, T5>(param1, param2, param3, param4, param5), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, scheduler, options).Task;
		}
		public static Task Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { return Create<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler) { return Create<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options) { return Create<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, null, options); }
		public static Task StartNew<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { return StartNew<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler) { return StartNew<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options) { return StartNew<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, null, options); }
		#endregion
		#region Action<T1,T2, T3, T4, T5, T6>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2, T3, T4, T5, T6>;
				action.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2, T3, T4, T5, T6>(param1, param2, param3, param4, param5, param6), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, scheduler, options).Task;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { return Create<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler) { return Create<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options) { return Create<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, null, options); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { return StartNew<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler) { return StartNew<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options) { return StartNew<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, null, options); }
		#endregion
		#region Action<T1,T2, T3, T4, T5, T6, T7>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2, T3, T4, T5, T6, T7>;
				action.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2, T3, T4, T5, T6, T7>(param1, param2, param3, param4, param5, param6, param7), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, scheduler, options).Task;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) { return Create<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler) { return Create<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskCreationOptions options) { return Create<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, null, options); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7) { return StartNew<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler) { return StartNew<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskCreationOptions options) { return StartNew<T1, T2, T3, T4, T5, T6, T7>(action, param1, param2, param3, param4, param5, param6, param7, null, options); }
		#endregion
		#region Action<T1,T2, T3, T4, T5, T6, T7, T8>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var actionObj = new Action<object>((obj) =>
			{
				//Los parámetros los recibiré como una tupla
				var tuple = obj as Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>;
				action.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7, tuple.Rest.Item1);
			});
			//Al crear la tarea paso los parámetros como una Tupla
			var entry = new ActionTaskManagerEntry(actionObj, new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(param1, param2, param3, param4, param5, param6, param7, new Tuple<T8>(param8)), scheduler, options);
			AddEntry(entry);
			return entry;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options).Task;
		}
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) { return Create<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, null, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler) { return Create<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, default(TaskCreationOptions)); }
		public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskCreationOptions options) { return Create<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, null, options); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options);
			entry.Start();
			return entry.Task;
		}
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8) { return StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, null, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler) { return StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, default(TaskCreationOptions)); }
		public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskCreationOptions options) { return StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, null, options); }
		#endregion
		#region Func<TResult>
		private static ITaskManagerEntry CreateEntry<TResult>(Func<TResult> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var entry = new FuncTaskManagerEntry<TResult>(func, scheduler, options);
			AddEntry(entry);
			return entry;
		}
        public static Task<TResult> Create<TResult>(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return (Task<TResult>)CreateEntry(func, scheduler, options).Task;
		}
        public static Task<TResult> Create<TResult>(Func<TResult> func) { return Create(func, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<TResult>(Func<TResult> func, TaskScheduler scheduler) { return Create(func, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<TResult>(Func<TResult> func, TaskCreationOptions options) { return Create(func, null, options); }
        public static Task<TResult> StartNew<TResult>(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(func, scheduler, options);
			entry.Start();
			return (Task<TResult>)entry.Task;
		}
        public static Task<TResult> StartNew<TResult>(Func<TResult> func) { return StartNew(func, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<TResult>(Func<TResult> func, TaskScheduler scheduler) { return StartNew(func, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<TResult>(Func<TResult> func, TaskCreationOptions options) { return StartNew(func, null, options); }
		#endregion
		#region Func<T, TResult>
		private static ITaskManagerEntry CreateEntry<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var funcObj = new Func<object, TResult>((obj) => { return func.Invoke((T)obj); });
			var entry = new FuncTaskManagerEntry<TResult>(funcObj, param, scheduler, options);
			AddEntry(entry);
			return entry;

		}
        public static Task<TResult> Create<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return (Task<TResult>)CreateEntry(func, param, scheduler, options).Task;
		}
        public static Task<TResult> Create<T, TResult>(Func<T, TResult> func, T param) { return Create(func, param, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler) { return Create(func, param, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T, TResult>(Func<T, TResult> func, T param, TaskCreationOptions options) { return Create(func, param, null, options); }
		public static Task<TResult> StartNew<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(func, param, scheduler, options);
			entry.Start();
			return (Task<TResult>)entry.Task;
		}
		public static Task<TResult> StartNew<T, TResult>(Func<T, TResult> func, T param) { return StartNew(func, param, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler) { return StartNew(func, param, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T, TResult>(Func<T, TResult> func, T param, TaskCreationOptions options) { return StartNew(func, param, null, options); }
		#endregion
		#region Func<T1, T2, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var funcObj = new Func<object, TResult>((obj) =>
			{
				var tuple = obj as Tuple<T1, T2>;
				return func.Invoke(tuple.Item1, tuple.Item2);
			});
			var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2>(param1, param2), scheduler, options);
			AddEntry(entry);
			return entry;

		}
        public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler, TaskCreationOptions options)
		{
            return (Task<TResult>)CreateEntry(func, param1, param2, scheduler, options).Task;
		}
        public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2) { return Create(func, param1, param2, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler) { return Create(func, param1, param2, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskCreationOptions options) { return Create(func, param1, param2, null, options); }
        public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(func, param1, param2, scheduler, options);
			entry.Start();
			return (Task<TResult>)entry.Task;
		}
        public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2) { return StartNew(func, param1, param2, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler) { return StartNew(func, param1, param2, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskCreationOptions options) { return StartNew(func, param1, param2, null, options); }
		#endregion
		#region Func<T1, T2, T3, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var funcObj = new Func<object, TResult>((obj) =>
			{
				var tuple = obj as Tuple<T1, T2, T3>;
				return func.Invoke(tuple.Item1, tuple.Item2, tuple.Item3);
			});
			var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2, T3>(param1, param2, param3), scheduler, options);
			AddEntry(entry);
			return entry;

		}
        public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return (Task<TResult>)CreateEntry(func, param1, param2, param3, scheduler, options).Task;
		}
        public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) { return Create(func, param1, param2, param3, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler) { return Create(func, param1, param2, param3, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskCreationOptions options) { return Create(func, param1, param2, param3, null, options); }
        public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(func, param1, param2, param3, scheduler, options);
			entry.Start();
			return (Task<TResult>)entry.Task;
		}
        public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) { return StartNew(func, param1, param2, param3, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler) { return StartNew(func, param1, param2, param3, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskCreationOptions options) { return StartNew(func, param1, param2, param3, null, options); }
		#endregion
		#region Func<T1, T2, T3, T4, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var funcObj = new Func<object, TResult>((obj) =>
			{
				var tuple = obj as Tuple<T1, T2, T3, T4>;
				return func.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
			});
			var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2, T3, T4>(param1, param2, param3, param4), scheduler, options);
			AddEntry(entry);
			return entry;

		}
        public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, scheduler, options).Task;
		}
        public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4) { return Create(func, param1, param2, param3, param4, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler) { return Create(func, param1, param2, param3, param4, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options) { return Create(func, param1, param2, param3, param4, null, options); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(func, param1, param2, param3, param4, scheduler, options);
			entry.Start();
			return (Task<TResult>)entry.Task;
		}
        public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4) { return StartNew(func, param1, param2, param3, param4, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler) { return StartNew(func, param1, param2, param3, param4, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskCreationOptions options) { return StartNew(func, param1, param2, param3, param4, null, options); }
		#endregion
		#region Func<T1, T2, T3, T4, T5, TResult>
		private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
		{
			var funcObj = new Func<object, TResult>((obj) =>
			{
				var tuple = obj as Tuple<T1, T2, T3, T4, T5>;
				return func.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
			});
			var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2, T3, T4, T5>(param1, param2, param3, param4, param5), scheduler, options);
			AddEntry(entry);
			return entry;

		}
        public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler, TaskCreationOptions options)
		{
			return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, scheduler, options).Task;
		}
        public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { return Create(func, param1, param2, param3, param4, param5, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler) { return Create(func, param1, param2, param3, param4, param5, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options) { return Create(func, param1, param2, param3, param4, param5, null, options); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler, TaskCreationOptions options)
		{
			var entry = CreateEntry(func, param1, param2, param3, param4, param5, scheduler, options);
			entry.Start();
			return (Task<TResult>)entry.Task;
		}
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { return StartNew(func, param1, param2, param3, param4, param5, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler) { return StartNew(func, param1, param2, param3, param4, param5, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskCreationOptions options) { return StartNew(func, param1, param2, param3, param4, param5, null, options); }
		#endregion
        #region Func<T1, T2, T3, T4, T5, T6, TResult>
        private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var funcObj = new Func<object, TResult>((obj) =>
            {
                var tuple = obj as Tuple<T1, T2, T3, T4, T5, T6>;
                return func.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
            });
            var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2, T3, T4, T5, T6>(param1, param2, param3, param4, param5, param6), scheduler, options);
            AddEntry(entry);
            return entry;

        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler, TaskCreationOptions options)
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { return Create(func, param1, param2, param3, param4, param5, param6, null, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler) { return Create(func, param1, param2, param3, param4, param5, param6, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options) { return Create(func, param1, param2, param3, param4, param5, param6, null, options); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler, TaskCreationOptions options)
        {
            var entry = CreateEntry(func, param1, param2, param3, param4, param5, param6, scheduler, options);
            entry.Start();
            return (Task<TResult>)entry.Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { return StartNew(func, param1, param2, param3, param4, param5, param6, null, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler) { return StartNew(func, param1, param2, param3, param4, param5, param6, scheduler, default(TaskCreationOptions)); }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskCreationOptions options) { return StartNew(func, param1, param2, param3, param4, param5, param6, null, options); }
        #endregion
		internal interface ITaskManagerEntry
		{
			Task Task { get; }
			bool IsCancellationRequested { get; }
			TaskScheduler TaskScheduler { get; }
			TaskCreationOptions TaskCreationOptions { get; }
			AutoResetEvent AutoResetEvent { get; }
			bool IsSleeping { get; set; }
			event EventHandler Canceled;
			void Cancel();
			void Start();
		}
		abstract class TaskManagerEntry : ITaskManagerEntry
		{
			protected TaskManagerEntry(TaskScheduler scheduler, TaskCreationOptions options)
			{
				CancellationTokenSource = new CancellationTokenSource();
				AutoResetEvent = new AutoResetEvent(false);
				TaskScheduler = scheduler ?? TaskScheduler.Default;
				TaskCreationOptions = options;
			}

			private ILog log = LogManager.Create(typeof(TaskManager));
            public Task Task { get; set; }
			public TaskScheduler TaskScheduler { get; set; }
			public TaskCreationOptions TaskCreationOptions { get; set; }
			public AutoResetEvent AutoResetEvent { get; set; }
			public bool IsSleeping { get; set; }

			public void Start()
			{
				Task.ContinueWith(t =>
				{
					string toLog = "La tarea finalizó con " + t.Exception.InnerExceptions.Count + " errores.";
					if (t.Exception.InnerExceptions.Count == 1)
					{
						toLog += " Error '" + t.Exception.InnerException.GetType().Name + "': " + t.Exception.InnerException.Message;
					}
					log.Error(toLog, t.Exception);
				}, TaskContinuationOptions.OnlyOnFaulted);
				Task.Start(TaskScheduler);
			}

			public event EventHandler Canceled;
			public bool IsCancellationRequested { get { return CancellationTokenSource.IsCancellationRequested; } }
			protected CancellationTokenSource CancellationTokenSource { get; set; }
			public void Cancel()
			{
				CancellationTokenSource.Cancel();
				if (Canceled != null) Canceled(this, EventArgs.Empty);
			}
			
		}
		class ActionTaskManagerEntry : TaskManagerEntry
		{
			public ActionTaskManagerEntry(Action action, TaskScheduler scheduler, TaskCreationOptions options)
				: base(scheduler, options)
			{
				Task = new Task(action, CancellationTokenSource.Token, TaskCreationOptions);
			}
			public ActionTaskManagerEntry(Action action) : this(action, (TaskScheduler)null, default(TaskCreationOptions)) { }
			public ActionTaskManagerEntry(Action action, TaskScheduler scheduler) : this(action, scheduler, default(TaskCreationOptions)) { }
			public ActionTaskManagerEntry(Action action, TaskCreationOptions options) : this(action, (TaskScheduler)null, options) { }
			public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler, TaskCreationOptions options)
				: base((TaskScheduler)null, default(TaskCreationOptions))
			{
				Task = new Task(action, state, CancellationTokenSource.Token, TaskCreationOptions);
			}
			public ActionTaskManagerEntry(Action<object> action, object state) : this(action, state, null, default(TaskCreationOptions)) { }
			public ActionTaskManagerEntry(Action<object> action, object state, TaskScheduler scheduler) : this(action, state, scheduler, default(TaskCreationOptions)) { }
			public ActionTaskManagerEntry(Action<object> action, object state, TaskCreationOptions options) : this(action, state, (TaskScheduler)null, options) { }
		}
		class FuncTaskManagerEntry<TResult> : TaskManagerEntry
		{
			public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler scheduler, TaskCreationOptions options)
				: base(scheduler, options)
			{
				Task = new Task<TResult>(func, CancellationTokenSource.Token, TaskCreationOptions);
			}
			public FuncTaskManagerEntry(Func<TResult> func) : this(func, (TaskScheduler)null, default(TaskCreationOptions)) { }
			public FuncTaskManagerEntry(Func<TResult> func, TaskScheduler scheduler) : this(func, scheduler, default(TaskCreationOptions)) { }
			public FuncTaskManagerEntry(Func<TResult> func, TaskCreationOptions options) : this(func, (TaskScheduler)null, options) { }
			public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler, TaskCreationOptions options)
				: base((TaskScheduler)null, default(TaskCreationOptions))
			{
				Task = new Task<TResult>(func, state, CancellationTokenSource.Token, TaskCreationOptions);
			}
			public FuncTaskManagerEntry(Func<object, TResult> func, object state) : this(func, state, null, default(TaskCreationOptions)) { }
			public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskScheduler scheduler) : this(func, state, scheduler, default(TaskCreationOptions)) { }
			public FuncTaskManagerEntry(Func<object, TResult> func, object state, TaskCreationOptions options) : this(func, state, (TaskScheduler)null, options) { }
		}
	}
}
