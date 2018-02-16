using Fuxion.Logging;
using Fuxion.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fuxion.Threading
{
	//public delegate void ActionRef<T>(ref T obj);
	//public delegate void ActionRef<T, U>(ref T obj, U param1);
	//public delegate TResult FuncRef<T, TResult>(ref T arg);
	//public interface ILocker<out TObjectLocked>
	//{
	//	void Read(Action<TObjectLocked> action);
	//	TResult Read<TResult>(Func<TObjectLocked, TResult> func);
	//	void Write(Action<TObjectLocked> action);
	//	TResult Write<TResult>(Func<TObjectLocked, TResult> func);
	//}
	public class Locker<TObjectLocked>
	{
		public Locker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) { this.objectLocked = objectLocked; }
		ReaderWriterLockSlim _ReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		TObjectLocked objectLocked;
        ILog log = LogManager.Create<Locker<TObjectLocked>>();
		//public TObjectLocked ObjectLocked
		//{
		//	get { return Read(obj => obj); }
		//    protected set
		//    {
  //              _ReaderWriterLockSlim.EnterWriteLock();
		//        objectLocked = value;
  //              _ReaderWriterLockSlim.ExitWriteLock();
		//    }
		//}
		public void Read(Action<TObjectLocked> action)
		{
            try
            {
                _ReaderWriterLockSlim.EnterReadLock();
                action.Invoke(objectLocked);
            }
            catch (Exception ex)
            {
                log.Error($"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _ReaderWriterLockSlim.ExitReadLock();
            }
		}
		public TResult Read<TResult>(Func<TObjectLocked, TResult> func)
		{
            try
            {
                _ReaderWriterLockSlim.EnterReadLock();
                TResult res = func.Invoke(objectLocked);
                return res;
            }
            catch (Exception ex)
            {
                log.Error($"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _ReaderWriterLockSlim.ExitReadLock();
            }
		}
		public void Write(Action<TObjectLocked> action)
		{
            try
            {
                _ReaderWriterLockSlim.EnterWriteLock();
                action.Invoke(objectLocked);
            }
            catch (Exception ex)
            {
                log.Error($"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}", ex);
                throw;
            }
            finally
            {
                _ReaderWriterLockSlim.ExitWriteLock();
            }
		}
        public TResult Write<TResult>(Func<TObjectLocked, TResult> func)
        {
            try
            {
                _ReaderWriterLockSlim.EnterWriteLock();
                TResult res = func.Invoke(objectLocked);
                return res;
            }
            catch (Exception ex)
            {
                log.Error($"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}", ex);
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
		private Task DelegateReadAsync(Delegate del, params object[] pars)
        {
            return TaskManager.StartNew((d, ps) =>
            {
                try
                {
                    _ReaderWriterLockSlim.EnterReadLock();
                    d.DynamicInvoke(objectLocked, ps);
                }
                catch (Exception ex)
                {
                    log.Error($"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}", ex);
                    throw;
                }
                finally
                {
                    _ReaderWriterLockSlim.ExitReadLock();
                }
            }, del, pars);
        }
		private Task<TResult> DelegateReadAsync<TResult>(Delegate del, params object[] pars)
        {
            return TaskManager.StartNew((d, ps) =>
            {
                try
                {
                    _ReaderWriterLockSlim.EnterReadLock();
                    return (TResult)d.DynamicInvoke(objectLocked, ps);
                }
                catch (Exception ex)
                {
                    log.Error($"Error '{ex.GetType().Name}' in Locker.Read: {ex.Message}", ex);
                    throw;
                }
                finally
                {
                    _ReaderWriterLockSlim.ExitReadLock();
                }
            }, del, pars);
        }
        private Task DelegateWriteAsync(Delegate del, params object[] pars)
        {
            return TaskManager.StartNew((d,ps) =>
            {
                try
                {
                    _ReaderWriterLockSlim.EnterWriteLock();
                    d.DynamicInvoke(ps);
                }
                catch (Exception ex)
                {
                    log.Error($"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}", ex);
                    throw;
                }
                finally
                {
                    _ReaderWriterLockSlim.ExitWriteLock();
                }
            }, del, pars);
        }
        private Task<TResult> DelegateWriteAsync<TResult>(Delegate del, params object[] pars)
        {
            return TaskManager.StartNew((d, ps) =>
            {
                try
                {
                    _ReaderWriterLockSlim.EnterWriteLock();
                    return (TResult)d.DynamicInvoke(pars);
                }
                catch (Exception ex)
                {
                    log.Error($"Error '{ex.GetType().Name}' in Locker.Write: {ex.Message}", ex);
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
        public Task ReadAsync(Action<TObjectLocked> action) { return DelegateReadAsync(action); }
        public Task ReadAsync<T>(Action<TObjectLocked, T> action, T param) { return DelegateReadAsync(action, param); }
        public Task ReadAsync<T1, T2>(Action<TObjectLocked, T1, T2> action, T1 param1, T2 param2) { return DelegateReadAsync(action, param1, param2); }
        public Task ReadAsync<T1, T2, T3>(Action<TObjectLocked, T1, T2, T3> action, T1 param1, T2 param2, T3 param3) { return DelegateReadAsync(action, param1, param2, param3); }

        // Read function
        public Task<TResult> ReadSync<TResult>(Func<TObjectLocked, TResult> func) { return DelegateReadAsync<TResult>(func); }
        public Task<TResult> ReadSync<T, TResult>(Func<TObjectLocked, T, TResult> func, T param) { return DelegateReadAsync<TResult>(func, param); }
        public Task<TResult> ReadSync<T1, T2, TResult>(Func<TObjectLocked, T1, T2, TResult> func, T1 param1, T2 param2) { return DelegateReadAsync<TResult>(func, param1, param2); }
        public Task<TResult> ReadSync<T1, T2, T3, TResult>(Func<TObjectLocked, T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) { return DelegateReadAsync<TResult>(func, param1, param2, param3); }

        // Write action
        public Task WriteAsync(Action<TObjectLocked> action) { return DelegateWriteAsync(action); }
        public Task WriteAsync<T>(Action<TObjectLocked, T> action, T param) { return DelegateWriteAsync(action, param); }
        public Task WriteAsync<T1, T2>(Action<TObjectLocked, T1, T2> action, T1 param1, T2 param2) { return DelegateWriteAsync(action, param1, param2); }
        public Task WriteAsync<T1, T2, T3>(Action<TObjectLocked, T1, T2, T3> action, T1 param1, T2 param2, T3 param3) { return DelegateWriteAsync(action, param1, param2, param3); }

        // Write function
        public Task<TResult> WriteAsync<TResult>(Func<TObjectLocked, TResult> func) { return DelegateWriteAsync<TResult>(func); }
        public Task<TResult> WriteAsync<T, TResult>(Action<TObjectLocked, T, TResult> func, T param) { return DelegateWriteAsync<TResult>(func, param); }
        public Task<TResult> WriteAsync<T1, T2, TResult>(Action<TObjectLocked, T1, T2, TResult> func, T1 param1, T2 param2) { return DelegateWriteAsync<TResult>(func, param1, param2); }
        public Task<TResult> WriteAsync<T1, T2, T3, TResult>(Action<TObjectLocked, T1, T2, T3, TResult> func, T1 param1, T2 param2, T3 param3) { return DelegateWriteAsync<TResult>(func, param1, param2, param3); }
        #endregion

    }
	//public class ValueLocker<TObjectLocked> : BaseLocker<TObjectLocked>, ILocker<TObjectLocked>
	//{
	//	public ValueLocker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) : base(objectLocked, recursionPolicy) { }
	//    public void WriteRef(TObjectLocked value) { ObjectLocked = value; }
	//}
	//public class RefLocker<TObjectLocked> : BaseLocker<TObjectLocked>, ILocker<TObjectLocked>
	//{
	//	public RefLocker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) : base(objectLocked, recursionPolicy) { }
	//}
}
