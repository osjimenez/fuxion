using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.ComponentModel
{
    [FactoryDefaultImplementation(typeof(DefaultNotifierSynchronizer))]
    public interface INotifierSynchronizer
    {
        #region Invoke Actions
        Task Invoke(Action action);
        Task Invoke<T>(Action<T> action, T param);
        Task Invoke<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2);
        Task Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3);
        Task Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4);
        Task Invoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
        Task Invoke<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);
        #endregion
        #region Invoke Funcs
        Task<TResult> Invoke<TResult>(Func<TResult> func);
        Task<TResult> Invoke<T, TResult>(Func<T, TResult> func, T param);
        Task<TResult> Invoke<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2);
        Task<TResult> Invoke<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3);
        Task<TResult> Invoke<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4);
        Task<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
        Task<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);
        #endregion
    }
}
