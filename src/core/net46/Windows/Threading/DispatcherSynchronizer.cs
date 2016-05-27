using Fuxion.ComponentModel;
using Fuxion.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Fuxion.Windows.Threading
{
    public class DispatcherSynchronizer : INotifierSynchronizer
    {
        public DispatcherSynchronizer()
        {
            if (SynchronizationContext.Current != null)
                Dispatcher = Dispatcher.CurrentDispatcher;
        }
        public DispatcherSynchronizer(Dispatcher dispatcher) : this() { Dispatcher = dispatcher; }
        public Dispatcher Dispatcher { get; set; }

        private Task InvokeActionDelegate(Delegate method, params object[] args)
        {
            if (Dispatcher == null) // No dispatcher, runs method direct and dynamically
                return Task.FromResult(method.DynamicInvoke(args));
            if (Dispatcher.CheckAccess()) // I'm in the same thread of dispatcher, runs synchronously
                return Task.FromResult(Dispatcher.Invoke(method, args));
            else // I'm in another thread than dispatcher, runs asynchronously
                return Dispatcher.BeginInvoke(method, args).Task;
        }
        private Task<TResult> InvokeFuncDelegate<TResult>(Delegate method, params object[] args)
        {
            if (Dispatcher == null) // No dispatcher, runs method direct and dynamically
                return Task.FromResult((TResult)method.DynamicInvoke(args));
            if (Dispatcher.CheckAccess()) // I'm in the same thread of dispatcher, runs synchronously
                return Task.FromResult((TResult)Dispatcher.Invoke(method, args));
            else // I'm in another thread than dispatcher, runs asynchronously
            {
                var ope = Dispatcher.BeginInvoke(method, args);
                return ope.Task.ContinueWith((t, o) => ((Task<TResult>)o).Result, ope, TaskContinuationOptions.ExecuteSynchronously);
            }
        }
        #region Invoke Actions
        public async Task Invoke(Action action) { await InvokeActionDelegate(action); }
        public async Task Invoke<T>(Action<T> action, T param) { await InvokeActionDelegate(action, param); }
        public async Task Invoke<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2) { await InvokeActionDelegate(action, param1, param2); }
        public async Task Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) { await InvokeActionDelegate(action, param1, param2, param3); }
        public async Task Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4) { await InvokeActionDelegate(action, param1, param2, param3, param4); }
        public async Task Invoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { await InvokeActionDelegate(action, param1, param2, param3, param4, param5); }
        public async Task Invoke<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { await InvokeActionDelegate(action, param1, param2, param3, param4, param5, param6); }
        #endregion
        #region Invoke Funcs
        public async Task<TResult> Invoke<TResult>(Func<TResult> func) { return await InvokeFuncDelegate<TResult>(func); }
        public async Task<TResult> Invoke<T, TResult>(Func<T, TResult> func, T param) { return await InvokeFuncDelegate<TResult>(func, param); }
        public async Task<TResult> Invoke<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2) { return await InvokeFuncDelegate<TResult>(func, param1, param2); }
        public async Task<TResult> Invoke<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) { return await InvokeFuncDelegate<TResult>(func, param1, param2, param3); }
        public async Task<TResult> Invoke<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4) { return await InvokeFuncDelegate<TResult>(func, param1, param2, param3, param4); }
        public async Task<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) { return await InvokeFuncDelegate<TResult>(func, param1, param2, param3, param4, param5); }
        public async Task<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) { return await InvokeFuncDelegate<TResult>(func, param1, param2, param3, param4, param5, param6); }
        #endregion
    }
}
