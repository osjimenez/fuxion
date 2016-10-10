using Fuxion.Logging;
using System;
using System.Threading;
namespace Fuxion.Threading
{
	public delegate void ActionRef<T>(ref T obj);
	public delegate void ActionRef<T, U>(ref T obj, U param1);
	public delegate TResult FuncRef<T, TResult>(ref T arg);
	public interface ILocker<out TObjectLocked>
	{
		void Read(Action<TObjectLocked> action);
		TResult Read<TResult>(Func<TObjectLocked, TResult> func);
		void Write(Action<TObjectLocked> action);
		TResult Write<TResult>(Func<TObjectLocked, TResult> func);
	}
	public abstract class BaseLocker<TObjectLocked>
	{
		public BaseLocker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) { this.objectLocked = objectLocked; }
		ReaderWriterLockSlim _ReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		TObjectLocked objectLocked;
        ILog log = LogManager.Create<BaseLocker<TObjectLocked>>();
		public TObjectLocked ObjectLocked
		{
			get { return Read(obj => obj); }
		    protected set
		    {
                _ReaderWriterLockSlim.EnterWriteLock();
		        objectLocked = value;
                _ReaderWriterLockSlim.ExitWriteLock();
		    }
		}
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
	}
	public class ValueLocker<TObjectLocked> : BaseLocker<TObjectLocked>
	{
		public ValueLocker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) : base(objectLocked, recursionPolicy) { }
	    public void WriteRef(TObjectLocked value) { ObjectLocked = value; }
	}
	public class RefLocker<TObjectLocked> : BaseLocker<TObjectLocked>, ILocker<TObjectLocked>
	{
		public RefLocker(TObjectLocked objectLocked, LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion) : base(objectLocked, recursionPolicy) { }
	}
}
