using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
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
                //if (entry.IsSleeping) entry.AutoResetEvent.Set();
            }
        }
        public static void CancelAndWait(this Task task, TimeSpan timeout = default(TimeSpan), bool throwExceptionIfNotRunning = true)  => new[] { task }.CancelAndWait(timeout, throwExceptionIfNotRunning);
        public static void CancelAndWait(this IEnumerable<Task> me, TimeSpan timeout = default(TimeSpan), bool throwExceptionIfNotRunning = true) {
            foreach (var task in me)
                task.Cancel(throwExceptionIfNotRunning);
            try
            {
                if (timeout != default(TimeSpan))
                    Task.WaitAll(me.Where(t => t != null && !t.IsCanceled).ToArray(), timeout);
                else
                    Task.WaitAll(me.Where(t => t != null && !t.IsCanceled).ToArray());
            }
            catch (AggregateException aex) when (aex.InnerException is TaskCanceledException) { }
            //catch (TaskCanceledException) { }
        }

        public static void OnCancel(this Task task, Action action) => TaskManager.SearchEntry(task, true).Canceled += (s, e) => action();
        public static bool IsCancellationRequested(this Task task, bool throwExceptionIfNotRunning = false)
        {
            var item = TaskManager.SearchEntry(task, throwExceptionIfNotRunning);
            if (item != null)
            {
                return item.IsCancellationRequested;
            }
            else
                if (throwExceptionIfNotRunning) throw new ArgumentException("IsCancellationRequested: La tarea no esta administrada por el TaskManager." + task.CreationOptions.ToString());
            else return false;
        }
        public static CancellationToken GetCancellationToken(this Task task, bool throwExceptionIfNotRunning = false)
        {
            var item = TaskManager.SearchEntry(task, throwExceptionIfNotRunning);
            return item.CancellationTokenSource.Token;
        }
        public static TResult WaitResult<TResult>(this Task<TResult> task, TimeSpan timeout = default(TimeSpan), bool rethrowException = true)
        {
            try
            {
                var tt = task.ConfigureAwait(false);
                if (timeout == default(TimeSpan)) task.Wait();
                else task.Wait(timeout);
                return task.Result;
            }
            catch (AggregateException aex)
            {
                if (rethrowException)
                {
                    aex = aex.Flatten();
                    if (aex.InnerExceptions.Count == 1) throw aex.InnerException;
                    throw aex;
                }
                else
                    return default(TResult);
            }
        }
        //public static void Sleep(this Task task, TimeSpan timeout, TimeSpan checkCancellationRequestInterval = default(TimeSpan))
        //{
        //    var entry = TaskManager.SearchEntry(task);
        //    entry.IsSleeping = true;
        //    if(checkCancellationRequestInterval == default(TimeSpan))
        //        entry.AutoResetEvent.WaitOne(timeout);
        //    else
        //    {
        //        var limit = DateTime.Now.Add(timeout);
        //        while(DateTime.Now < limit && !entry.IsCancellationRequested)
        //        {
        //            entry.AutoResetEvent.WaitOne(checkCancellationRequestInterval);
        //        }
        //    }
        //    entry.IsSleeping = false;
        //}
        public static void Sleep(this Task task, TimeSpan timeout)
        {
            var entry = TaskManager.SearchEntry(task);
            try
            {
                Task.Delay(timeout, entry.CancellationTokenSource.Token).Wait();
            }
            catch (AggregateException aex) when (aex.InnerException is TaskCanceledException) { }
            //entry.IsSleeping = true;
            //if (checkCancellationRequestInterval == default(TimeSpan))
            //    entry.AutoResetEvent.WaitOne(timeout);
            //else
            //{
            //    var limit = DateTime.Now.Add(timeout);
            //    while (DateTime.Now < limit && !entry.IsCancellationRequested)
            //    {
            //        entry.AutoResetEvent.WaitOne(checkCancellationRequestInterval);
            //    }
            //}
            //entry.IsSleeping = false;
        }
    }
}
