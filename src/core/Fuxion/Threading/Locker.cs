using Fuxion.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Fuxion.Threading
{
	public class Locker<TObjectLocked> : IDisposable
	{
		public Locker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) => this.objectLocked = objectLocked;

		private readonly ReaderWriterLockSlim _ReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private TObjectLocked objectLocked;
		public ILogger? Logger { get; set; }

		public void Dispose() => _ReaderWriterLockSlim.Dispose();

		public void Read(Action<TObjectLocked> action)
		{
			_ReaderWriterLockSlim.EnterReadLock();
			try
			{
				action.Invoke(objectLocked);
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}");
				throw;
			}
			finally
			{
				_ReaderWriterLockSlim.ExitReadLock();
			}
		}
		public void ReadUpgradeable(Action<TObjectLocked> action)
		{
			_ReaderWriterLockSlim.EnterUpgradeableReadLock();
			try
			{
				action.Invoke(objectLocked);
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}");
				throw;
			}
			finally
			{
				_ReaderWriterLockSlim.ExitUpgradeableReadLock();
			}
		}
		public TResult Read<TResult>(Func<TObjectLocked, TResult> func)
		{
			_ReaderWriterLockSlim.EnterReadLock();
			try
			{
				var res = func.Invoke(objectLocked);
				return res;
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}");
				throw;
			}
			finally
			{
				_ReaderWriterLockSlim.ExitReadLock();
			}
		}
		public TResult ReadUpgradeable<TResult>(Func<TObjectLocked, TResult> func)
		{
			_ReaderWriterLockSlim.EnterUpgradeableReadLock();
			try
			{
				var res = func.Invoke(objectLocked);
				return res;
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}");
				throw;
			}
			finally
			{
				_ReaderWriterLockSlim.ExitUpgradeableReadLock();
			}
		}
		public void Write(Action<TObjectLocked> action)
		{
			_ReaderWriterLockSlim.EnterWriteLock();
			try
			{
				action.Invoke(objectLocked);
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}");
				throw;
			}
			finally
			{
				_ReaderWriterLockSlim.ExitWriteLock();
			}
		}
		public TResult Write<TResult>(Func<TObjectLocked, TResult> func)
		{
			_ReaderWriterLockSlim.EnterWriteLock();
			try
			{
				var res = func.Invoke(objectLocked);
				return res;
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}");
				throw;
			}
			finally
			{
				_ReaderWriterLockSlim.ExitWriteLock();
			}
		}
		public void WriteObject(TObjectLocked value)
		{
			_ReaderWriterLockSlim.EnterWriteLock();
			objectLocked = value;
			_ReaderWriterLockSlim.ExitWriteLock();
		}
		#region Async delegates
		private Task DelegateReadAsync(Delegate del, params object?[] pars)
		{
			return TaskManager.StartNew((d, ps) =>
			{
				_ReaderWriterLockSlim.EnterReadLock();
				try
				{
					var p = new object?[] { objectLocked }.ToList();
					p.AddRange(ps);
					d.DynamicInvoke(p.ToArray());
				}
				catch (Exception ex)
				{
					Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}");
					throw;
				}
				finally
				{
					_ReaderWriterLockSlim.ExitReadLock();
				}
			}, del, pars);
		}
		private Task<TResult> DelegateReadAsync<TResult>(Delegate del, params object?[] pars)
		{
			return TaskManager.StartNew((d, ps) =>
			{
				_ReaderWriterLockSlim.EnterReadLock();
				try
				{
					var p = new object?[] { objectLocked }.ToList();
					p.AddRange(ps);
					return (TResult)d.DynamicInvoke(p.ToArray());
				}
				catch (Exception ex)
				{
					Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}");
					throw;
				}
				finally
				{
					_ReaderWriterLockSlim.ExitReadLock();
				}
			}, del, pars);
		}
		private Task DelegateWriteAsync(Delegate del, params object?[] pars)
		{
			return TaskManager.StartNew((d, ps) =>
			{
				_ReaderWriterLockSlim.EnterWriteLock();
				try
				{
					var p = new object?[] { objectLocked }.ToList();
					p.AddRange(ps);
					d.DynamicInvoke(p.ToArray());
				}
				catch (Exception ex)
				{
					Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}");
					throw;
				}
				finally
				{
					_ReaderWriterLockSlim.ExitWriteLock();
				}
			}, del, pars);
		}
		private Task<TResult> DelegateWriteAsync<TResult>(Delegate del, params object?[] pars)
		{
			return TaskManager.StartNew((d, ps) =>
			{
				_ReaderWriterLockSlim.EnterWriteLock();
				try
				{
					var p = new object?[] { objectLocked }.ToList();
					p.AddRange(ps);
					return (TResult)d.DynamicInvoke(p.ToArray());
				}
				catch (Exception ex)
				{
					Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}");
					throw;
				}
				finally
				{
					_ReaderWriterLockSlim.ExitWriteLock();
				}
			}, del, pars);
		}
		#endregion
		#region Async methods
		// Read action
		public Task ReadAsync(Action<TObjectLocked> action) => DelegateReadAsync(action);
		public Task ReadAsync<T>(Action<TObjectLocked, T> action, T param) => DelegateReadAsync(action, param);
		//public Task ReadAsync<T>(Action<TObjectLocked, T> action, T param) where T : class => DelegateReadAsync(action, param);
		//public Task ReadAsync<T>(Action<TObjectLocked, T> action, T param) where T : struct => DelegateReadAsync(action, param);
		public Task ReadAsync<T1, T2>(Action<TObjectLocked, T1, T2> action, T1 param1, T2 param2) => DelegateReadAsync(action, param1, param2);
		public Task ReadAsync<T1, T2, T3>(Action<TObjectLocked, T1, T2, T3> action, T1 param1, T2 param2, T3 param3) => DelegateReadAsync(action, param1, param2, param3);

		// Read function
		public Task<TResult> ReadAsync<TResult>(Func<TObjectLocked, TResult> func) => DelegateReadAsync<TResult>(func);
		public Task<TResult> ReadAsync<T, TResult>(Func<TObjectLocked, T, TResult> func, T param) => DelegateReadAsync<TResult>(func, param);
		public Task<TResult> ReadAsync<T1, T2, TResult>(Func<TObjectLocked, T1, T2, TResult> func, T1 param1, T2 param2) => DelegateReadAsync<TResult>(func, param1, param2);
		public Task<TResult> ReadAsync<T1, T2, T3, TResult>(Func<TObjectLocked, T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) => DelegateReadAsync<TResult>(func, param1, param2, param3);

		// Write action
		public Task WriteAsync(Action<TObjectLocked> action) => DelegateWriteAsync(action);
		public Task WriteAsync(Func<TObjectLocked, Task> action) => DelegateWriteAsync(action);
		public Task WriteAsync<T>(Action<TObjectLocked, T> action, T param) => DelegateWriteAsync(action, param);
		public Task WriteAsync<T>(Func<TObjectLocked, T, Task> action, T param) => DelegateWriteAsync(action, param);
		public Task WriteAsync<T1, T2>(Action<TObjectLocked, T1, T2> action, T1 param1, T2 param2) => DelegateWriteAsync(action, param1, param2);
		public Task WriteAsync<T1, T2>(Func<TObjectLocked, T1, T2, Task> action, T1 param1, T2 param2) => DelegateWriteAsync(action, param1, param2);
		public Task WriteAsync<T1, T2, T3>(Action<TObjectLocked, T1, T2, T3> action, T1 param1, T2 param2, T3 param3) => DelegateWriteAsync(action, param1, param2, param3);
		public Task WriteAsync<T1, T2, T3>(Func<TObjectLocked, T1, T2, T3, Task> action, T1 param1, T2 param2, T3 param3) => DelegateWriteAsync(action, param1, param2, param3);

		// Write function
		public Task<TResult> WriteAsync<TResult>(Func<TObjectLocked, TResult> func) => DelegateWriteAsync<TResult>(func);
		public Task<TResult> WriteAsync<TResult>(Func<TObjectLocked, Task<TResult>> func) => DelegateWriteAsync<TResult>(func);
		public Task<TResult> WriteAsync<T, TResult>(Action<TObjectLocked, T, TResult> func, T param) => DelegateWriteAsync<TResult>(func, param);
		public Task<TResult> WriteAsync<T, TResult>(Action<TObjectLocked, T, Task<TResult>> func, T param) => DelegateWriteAsync<TResult>(func, param);
		public Task<TResult> WriteAsync<T1, T2, TResult>(Action<TObjectLocked, T1, T2, TResult> func, T1 param1, T2 param2) => DelegateWriteAsync<TResult>(func, param1, param2);
		public Task<TResult> WriteAsync<T1, T2, TResult>(Action<TObjectLocked, T1, T2, Task<TResult>> func, T1 param1, T2 param2) => DelegateWriteAsync<TResult>(func, param1, param2);
		public Task<TResult> WriteAsync<T1, T2, T3, TResult>(Action<TObjectLocked, T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) => DelegateWriteAsync<TResult>(func, param1, param2, param3);
		public Task<TResult> WriteAsync<T1, T2, T3, TResult>(Action<TObjectLocked, T1, T2, T3, Task<TResult>> func, T1 param1, T2 param2, T3 param3) => DelegateWriteAsync<TResult>(func, param1, param2, param3);
		#endregion
	}
}