using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fuxion.Logging;
namespace Fuxion.Threading.Tasks
{
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
        }
        internal static ITaskManagerEntry SearchEntry(Task task, bool throwExceptionIfNotFound = true)
        {
            //Busco entre las tareas administradas cual tiene el id
            var res = tasks.Read(l => l.FirstOrDefault(e => e.Task == task));
            //Compruebo si se encontró y debo lanzar una excepción
            if (res == null && throwExceptionIfNotFound)
                throw new ArgumentException("Task wasn't created with TaskManager.");
            return res;
        }

        #region Action
        private static ITaskManagerEntry CreateEntry(Action action, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var entry = new ActionTaskManagerEntry(action, scheduler, options);
            AddEntry(entry);
            return entry;
        }
        public static Task Create(Action action, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
            => CreateEntry(action, scheduler, options).Task;
        
        public static Task Create(Func<Task> asyncAction, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
            => CreateEntry(new Action(() => asyncAction().Wait()), scheduler, options).Task;
        public static Task StartNew(Action action, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew(Func<Task> asyncAction, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
        #region Action<T>
        private static ITaskManagerEntry CreateEntry<T>(Action<T> action, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var actionObj = new Action<object>((obj) => action((T)obj));
            var entry = new ActionTaskManagerEntry(actionObj, param, scheduler, options);
            AddEntry(entry);
            return entry;
        }
        public static Task Create<T>(Action<T> action, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(action, param, scheduler, options).Task;
        }
        public static Task Create<T>(Func<T, Task> asyncAction, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T>(p => asyncAction(p).Wait()), param, scheduler, options).Task;
        }
        public static Task StartNew<T>(Action<T> action, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T>(Func<T, Task> asyncAction, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(action, param1, param2, scheduler, options).Task;
        }
        public static Task Create<T1, T2>(Func<T1, T2, Task> asyncAction, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2>((p1, p2) => asyncAction(p1, p2).Wait()), param1, param2, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2>(Func<T1, T2, Task> asyncAction, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry<T1, T2, T3>(action, param1, param2, param3, scheduler, options).Task;
        }
        public static Task Create<T1, T2, T3>(Func<T1, T2, T3, Task> asyncAction, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2, T3>((p1, p2, p3) => asyncAction(p1, p2, p3).Wait()), param1, param2, param3, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, param3, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2, T3>(Func<T1, T2, T3, Task> asyncAction, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, param3, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry<T1, T2, T3, T4>(action, param1, param2, param3, param4, scheduler, options).Task;
        }
        public static Task Create<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2, T3, T4>((p1, p2, p3, p4) => asyncAction(p1, p2, p3, p4).Wait()), param1, param2, param3, param4, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, param3, param4, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, param3, param4, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry<T1, T2, T3, T4, T5>(action, param1, param2, param3, param4, param5, scheduler, options).Task;
        }
        public static Task Create<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2, T3, T4, T5>((p1, p2, p3, p4, p5) => asyncAction(p1, p2, p3, p4, p5).Wait()), param1, param2, param3, param4, param5, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, param3, param4, param5, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, param3, param4, param5, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry<T1, T2, T3, T4, T5, T6>(action, param1, param2, param3, param4, param5, param6, scheduler, options).Task;
        }
        public static Task Create<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2, T3, T4, T5, T6>((p1, p2, p3, p4, p5, p6) => asyncAction(p1, p2, p3, p4, p5, p6).Wait()), param1, param2, param3, param4, param5, param6, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, param3, param4, param5, param6, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, param3, param4, param5, param6, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7>((p1, p2, p3, p4, p5, p6, p7) => asyncAction(p1, p2, p3, p4, p5, p6, p7).Wait()), param1, param2, param3, param4, param5, param6, param7, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, param3, param4, param5, param6, param7, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, param3, param4, param5, param6, param7, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8>(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options).Task;
        }
        public static Task Create<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return CreateEntry(new Action<T1, T2, T3, T4, T5, T6, T7, T8>((p1, p2, p3, p4, p5, p6, p7, p8) => asyncAction(p1, p2, p3, p4, p5, p6, p7, p8).Wait()), param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options).Task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(action, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task StartNew<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> asyncAction, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncAction, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
        #region Func<TResult>
        private static ITaskManagerEntry CreateEntry<TResult>(Func<TResult> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var entry = new FuncTaskManagerEntry<TResult>(func, scheduler, options);
            AddEntry(entry);
            return entry;
        }
        public static Task<TResult> Create<TResult>(Func<TResult> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, scheduler, options).Task;
        }
        public static Task<TResult> Create<TResult>(Func<Task<TResult>> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<TResult>(() => func().Result)).Task;
        }
        public static Task<TResult> StartNew<TResult>(Func<TResult> func, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<TResult>(Func<Task<TResult>> asyncFunc, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
        #region Func<T, TResult>
        private static ITaskManagerEntry CreateEntry<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var entry = new FuncTaskManagerEntry<TResult>(
                new Func<object, TResult>((obj) =>
                {
                    return func((T)obj);
                }),
                param, scheduler, options);
            AddEntry(entry);
            return entry;
        }
        public static Task<TResult> Create<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param, scheduler, options).Task;
        }
        public static Task<TResult> Create<T, TResult>(Func<T, Task<TResult>> asyncFunc, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T, TResult>(p => asyncFunc(p).Result), param, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T, TResult>(Func<T, TResult> func, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T, TResult>(Func<T, Task<TResult>> asyncFunc, T param, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
        #region Func<T1, T2, TResult>
        private static ITaskManagerEntry CreateEntry<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var entry = new FuncTaskManagerEntry<TResult>(
                new Func<object, TResult>((obj) =>
                {
                    var tuple = obj as Tuple<T1, T2>;
                    return func(tuple.Item1, tuple.Item2);
                }),
                new Tuple<T1, T2>(param1, param2),
                scheduler, options);
            AddEntry(entry);
            return entry;
        }
        public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, TResult>(Func<T1, T2, Task<TResult>> asyncFunc, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, TResult>((p1, p2) => asyncFunc(p1, p2).Result), param1, param2, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param1, param2, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T1, T2, TResult>(Func<T1, T2, Task<TResult>> asyncFunc, T1 param1, T2 param2, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, T3, TResult>((p1, p2, p3) => asyncFunc(p1, p2, p3).Result), param1, param2, param3, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param1, param2, param3, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, TResult>(Func<T1, T2, T3, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, param3, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, TResult>((p1, p2, p3, p4) => asyncFunc(p1, p2, p3, p4).Result), param1, param2, param3, param4, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param1, param2, param3, param4, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, param3, param4, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, TResult>((p1, p2, p3, p4, p5) => asyncFunc(p1, p2, p3, p4, p5).Result), param1, param2, param3, param4, param5, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param1, param2, param3, param4, param5, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, param3, param4, param5, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
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
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, TResult>((p1, p2, p3, p4, p5, p6) => asyncFunc(p1, p2, p3, p4, p5, p6).Result), param1, param2, param3, param4, param5, param6, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param1, param2, param3, param4, param5, param6, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, param3, param4, param5, param6, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
        #region Func<T1, T2, T3, T4, T5, T6, T7, TResult>
        private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var funcObj = new Func<object, TResult>((obj) =>
            {
                var tuple = obj as Tuple<T1, T2, T3, T4, T5, T6, T7>;
                return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7);
            });
            var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2, T3, T4, T5, T6, T7>(param1, param2, param3, param4, param5, param6, param7), scheduler, options);
            AddEntry(entry);
            return entry;

        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, TResult>((p1, p2, p3, p4, p5, p6, p7) => asyncFunc(p1, p2, p3, p4, p5, p6, p7).Result), param1, param2, param3, param4, param5, param6, param7, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(func, param1, param2, param3, param4, param5, param6, param7, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, param3, param4, param5, param6, param7, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
        #region Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
        private static ITaskManagerEntry CreateEntry<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var funcObj = new Func<object, TResult>((obj) =>
            {
                var tuple = obj as Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>;
                return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7, tuple.Rest.Item1);
            });
            var entry = new FuncTaskManagerEntry<TResult>(funcObj, new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(param1, param2, param3, param4, param5, param6, param7, new Tuple<T8>(param8)), scheduler, options);
            AddEntry(entry);
            return entry;

        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options).Task;
        }
        public static Task<TResult> Create<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            return (Task<TResult>)CreateEntry(new Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>((p1, p2, p3, p4, p5, p6, p7, p8) => asyncFunc(p1, p2, p3, p4, p5, p6, p7, p8).Result), param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options).Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var entry = CreateEntry(func, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options);
            entry.Start();
            return (Task<TResult>)entry.Task;
        }
        public static Task<TResult> StartNew<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<TResult>> asyncFunc, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, TaskScheduler scheduler = null, TaskCreationOptions options = default(TaskCreationOptions))
        {
            var task = Create(asyncFunc, param1, param2, param3, param4, param5, param6, param7, param8, scheduler, options);
            SearchEntry(task).Start();
            return task;
        }
        #endregion
    }
}
