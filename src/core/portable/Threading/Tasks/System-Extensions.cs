using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Threading.Tasks
{
    public static class Extensions
    {

        public static void Cancel(this Task task, bool throwExceptionIfNotRunning = true)
        {
            //Busco la entrada de la tarea
            var entry = TaskManager.SearchEntry(task, throwExceptionIfNotRunning);
            if (entry != null)
            {
                //Cancelo la tarea
                entry.Cancel();
                //Si la tarea esta dormida la despierto para que se pueda cancelar
                if (entry.IsSleeping) entry.AutoResetEvent.Set();
            }
        }
        public static void CancelAndWait(this Task task, bool throwExceptionIfNotRunning = true) { new[] { task }.CancelAndWait(throwExceptionIfNotRunning); }


        public static void CancelAndWait(this IEnumerable<Task> tasks, bool throwExceptionIfNotRunning = true) {
            foreach (var task in tasks)
                task.Cancel(throwExceptionIfNotRunning);
            Task.WaitAll(tasks.Where(t => t != null).ToArray());
        }
        public static void OnCancel(this Task task, Action<object, EventArgs> action)
        {
            TaskManager.SearchEntry(task, true).Canceled += (s, e) => action.Invoke(s, e);
        }
        public static bool IsCancellationRequested(this Task task, bool throwExceptionIfNotRunning = false)
        {
            var item = TaskManager.SearchEntry(task);
            if (item != null)
            {
                return item.IsCancellationRequested;
            }
            else
                if (throwExceptionIfNotRunning) throw new ArgumentException("IsCancellationRequested: La tarea no esta administrada por el TaskManager." + task.CreationOptions.ToString()); // TODO ALMU: QUITAR "IsCancellationRequested:" Y "task.CreationOptions.ToString()"
            else return false;
        }
        public static TResult WaitResult<TResult>(this Task<TResult> task, TimeSpan timeout = default(TimeSpan), bool rethrowException = true)
        {
            try
            {
                if (timeout == TimeSpan.Zero) task.Wait();
                else task.Wait(timeout);
                return task.Result;
            }
            catch (AggregateException aex)
            {
                aex = aex.Flatten();
                if (aex.InnerExceptions.Count == 1) throw aex.InnerException;
                if (rethrowException)
                    throw aex;
                else
                    return default(TResult);
            }
        }
        public static void Sleep(this Task task, TimeSpan timeout)
        {
            var entry = TaskManager.SearchEntry(task);
            //if (entry != null)
            //{
            entry.IsSleeping = true;
            entry.AutoResetEvent.WaitOne(timeout);
            entry.IsSleeping = false;
            //} else throw new InvalidOperationException("No se puede usar Sleep en una tarea que no es administrada por el TaskManager.");
        }
    }
}
