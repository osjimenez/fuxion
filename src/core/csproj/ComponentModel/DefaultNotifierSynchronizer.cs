using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.ComponentModel
{

	public class DefaultNotifierSynchronizer : INotifierSynchronizer
	{
		public Task Invoke(Action action)
		{
			action();
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<TResult>(Func<TResult> func)
		{
			return Task.FromResult(func());
		}

		public Task Invoke<T>(Action<T> action, T param)
		{
			action(param);
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<T, TResult>(Func<T, TResult> func, T param)
		{
			return Task.FromResult(func(param));
		}

		public Task Invoke<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
		{
			action(param1, param2);
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 param1, T2 param2)
		{
			return Task.FromResult(func(param1, param2));
		}

		public Task Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
		{
			action(param1, param2, param3);
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3)
		{
			return Task.FromResult(func(param1, param2, param3));
		}

		public Task Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			action(param1, param2, param3, param4);
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			return Task.FromResult(func(param1, param2, param3, param4));
		}

		public Task Invoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
		{
			action(param1, param2, param3, param4, param5);
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
		{
			return Task.FromResult(func(param1, param2, param3, param4, param5));
		}

		public Task Invoke<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
		{
			action(param1, param2, param3, param4, param5, param6);
			return Task.CompletedTask;
		}

		public Task<TResult> Invoke<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
		{
			return Task.FromResult(func(param1, param2, param3, param4, param5, param6));
		}
	}
}
